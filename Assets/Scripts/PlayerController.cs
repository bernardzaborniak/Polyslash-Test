using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    public Transform cameraTransform;
    public Rigidbody rb;

    [Header("Controls")]
    public float mouseSensitivity;
    
    [Header("Physics Based Movement")]
    public float maxMovementSpeed;
    public float maxAcceleration;
    public float maxDecceleration;

    //used for movement and camera rotation
    float horizontalMouseMovement;
    float verticalMouseMovement;
    float verticalCameraRotation;
    Vector3 movementVector;


    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        #region Calculate Input

        //mouse input
        horizontalMouseMovement = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        verticalMouseMovement = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

      
        //movement input
        movementVector = Vector3.zero;
        float horizontalMovementInput = Input.GetAxis("Horizontal");
        float verticalMovementInput = Input.GetAxis("Vertical");
        movementVector = playerTransform.TransformDirection(new Vector3(horizontalMovementInput, 0f, verticalMovementInput)); //convert the movement vector from local player space into world space
        
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
            //accelerate
            if (accel.sqrMagnitude > maxAcceleration * maxAcceleration)
                accel = accel.normalized * maxAcceleration;
        }
        else
        {
            //deccelerate
            if (accel.sqrMagnitude > maxDecceleration * maxDecceleration)
                accel = accel.normalized * maxDecceleration;

        }

        rb.AddForce(accel, ForceMode.Acceleration);

        #endregion
    }
}
