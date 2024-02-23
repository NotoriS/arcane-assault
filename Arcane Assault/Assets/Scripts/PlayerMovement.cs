using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    private Vector2 _movementInput;
    private Vector2 _cameraInput;

    private PlayerInputActions _playerInputActions;
    private CharacterController _characterController;
    private Camera _playerCamera;

    [SerializeField] private float movementSpeed = 2.5f;
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private float jumpVelocity = 5f;
    [SerializeField] private float gravity = -9.81f;

    private float _verticalVelocity = 0f;

    private float _horizontalRotation = 0f;
    private float _verticalRotation = 0f;

    private void Awake()
    {
        _characterController = gameObject.GetComponent<CharacterController>();
        _playerCamera = gameObject.GetComponentInChildren<Camera>();

        _playerInputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _playerInputActions.Player.Enable();
        _playerInputActions.Player.Jump.performed += Jump;
    }

    private void OnDisable()
    {
        _playerInputActions.Player.Jump.performed -= Jump;
        _playerInputActions.Player.Disable();
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update()
    {
        _movementInput = _playerInputActions.Player.PlayerMovement.ReadValue<Vector2>();
        _cameraInput = _playerInputActions.Player.CameraMovement.ReadValue<Vector2>();
        
        HalfApplyGravity();
        MoveCamera();
        MovePlayer();
        HalfApplyGravity();
    }

    // Must be called before and after updating position.
    private void HalfApplyGravity()
    {
        _verticalVelocity += gravity * Time.deltaTime * 0.5f;
        if (_characterController.isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = gravity * Time.deltaTime * 0.5f;
        }
    }

    private void MoveCamera()
    {
        _horizontalRotation += _cameraInput.x * mouseSensitivity;
        _verticalRotation -= _cameraInput.y * mouseSensitivity;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -90f, 90f);
        
        transform.rotation = Quaternion.Euler(0, _horizontalRotation, 0);
        _playerCamera.transform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
    }
    
    private void MovePlayer()
    {
        float zMovement = _movementInput.y * movementSpeed * Time.deltaTime;
        float xMovement = _movementInput.x * movementSpeed * Time.deltaTime;
        float yMovement = _verticalVelocity * Time.deltaTime;
        
        Vector3 movement = new Vector3(xMovement, yMovement, zMovement);
        movement = transform.TransformDirection(movement);
        
        _characterController.Move(movement);
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (_characterController.isGrounded)
        {
            _verticalVelocity += jumpVelocity;
        }
    }
}
