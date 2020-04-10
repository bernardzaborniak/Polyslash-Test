using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    Transform playerTransform;
    [SerializeField]
    Transform cameraTransform;
    [SerializeField]
    Rigidbody rb;

    [Header("Controls")]
    [SerializeField]
    float mouseSensitivity;
    
    [Header("Physics Based Movement")]
    [SerializeField]
    float maxMovementSpeed;
    [SerializeField]
    float maxAcceleration;
    [SerializeField]
    float maxDecceleration;

    [Header("For Clicking Buttons")]
    [SerializeField]
    float clickButtonRaycastDistance;
    [SerializeField]
    LayerMask clickButtonRaycastLayermask;

    // For movement and camera rotation
    float horizontalMouseMovement;
    float verticalMouseMovement;
    float verticalCameraRotation;
    Vector3 movementVector;


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        #region Get Input

        // mouse input
        horizontalMouseMovement = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        verticalMouseMovement = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

      
        // movement input
        movementVector = Vector3.zero;
        float horizontalMovementInput = Input.GetAxis("Horizontal");
        float verticalMovementInput = Input.GetAxis("Vertical");
        movementVector = playerTransform.TransformDirection(new Vector3(horizontalMovementInput, 0f, verticalMovementInput)); //convert the movement vector from local player space into world space

        // interaction
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, clickButtonRaycastDistance, clickButtonRaycastLayermask))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact();
                }
            }
        }

        #endregion
    }

    private void FixedUpdate()
    {
        #region Apply Camera Rotation

        verticalCameraRotation -= verticalMouseMovement;
        verticalCameraRotation = Mathf.Clamp(verticalCameraRotation, -85f, 85f);

        cameraTransform.localRotation = Quaternion.Euler(verticalCameraRotation, 0f, 0f);
        rb.MoveRotation(rb.rotation * Quaternion.Euler(playerTransform.up * horizontalMouseMovement));

        #endregion

        #region Apply Movement

        Vector3 rbHorizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        Vector3 targetVelocity = movementVector * maxMovementSpeed;
        Vector3 deltaV = targetVelocity - rbHorizontalVelocity;
        Vector3 accel = deltaV / Time.fixedDeltaTime;

        if ((rbHorizontalVelocity + accel).sqrMagnitude > rbHorizontalVelocity.sqrMagnitude)
        {
            // accelerate
            if (accel.sqrMagnitude > maxAcceleration * maxAcceleration)
                accel = accel.normalized * maxAcceleration;
        }
        else
        {
            // deccelerate
            if (accel.sqrMagnitude > maxDecceleration * maxDecceleration)
                accel = accel.normalized * maxDecceleration;

        }

        rb.AddForce(accel, ForceMode.Acceleration);

        #endregion
    }

    
}
