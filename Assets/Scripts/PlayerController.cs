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
    CharacterController characterController;

    [Header("Controls")]
    [SerializeField]
    float mouseSensitivity;
    [SerializeField]
    float movementSpeed;


    float verticalCameraRotation;
    Vector3 currentGravityForce;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        #region lookAround

        float horizontalMouseMovement = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float verticalMouseMovement = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        verticalCameraRotation -= verticalMouseMovement;
        verticalCameraRotation = Mathf.Clamp(verticalCameraRotation, -85f, 85f);

        cameraTransform.localRotation = Quaternion.Euler(verticalCameraRotation, 0f, 0f);
        playerTransform.Rotate(playerTransform.up * horizontalMouseMovement);

        #endregion

        #region playerMovement

        Vector3 movementVector = Vector3.zero;

        float horizontalMovementInput = Input.GetAxis("Horizontal");
        float verticalMovementInput = Input.GetAxis("Vertical");
        movementVector = playerTransform.TransformDirection(new Vector3(horizontalMovementInput, 0f, verticalMovementInput)); //convert the movement vector from local player space into world space
  
        characterController.Move(movementVector * movementSpeed * Time.deltaTime);
        characterController.Move(new Vector3(0f, -0.5f, 0f)); //very simple simulated gravity
        #endregion
    }
}
