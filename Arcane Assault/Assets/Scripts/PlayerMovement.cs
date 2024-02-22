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

    [SerializeField] private float movementSpeed = 2.5f;
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private float jumpVelocity = 5f;
    [SerializeField] private float gravity = -9.81f;

    private float _verticalVelocity = 0f;

    private void Awake()
    {
        _characterController = gameObject.GetComponent<CharacterController>();

        _playerInputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        _playerInputActions.Player.Enable();
        _playerInputActions.Player.Jump.performed += Jump;
    }

    private void OnDisable()
    {
        _playerInputActions.Player.Jump.performed -= Jump;
        _playerInputActions.Player.Disable();
    }

    private void Update()
    {
        _movementInput = _playerInputActions.Player.PlayerMovement.ReadValue<Vector2>();
        _cameraInput = _playerInputActions.Player.CameraMovement.ReadValue<Vector2>();

        UpdateGravity();
        MoveCamera();
        MovePlayer();
        UpdateGravity();
    }

    // Must be called before and after updating position.
    private void UpdateGravity()
    {
        _verticalVelocity += gravity * Time.deltaTime * 0.5f;
        if (_characterController.isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = gravity * Time.deltaTime * 0.5f;
        }
    }

    private void MoveCamera()
    {
        // TODO
    }
    
    private void MovePlayer()
    {
        float zMovement = _movementInput.y * movementSpeed * Time.deltaTime;
        float xMovement = _movementInput.x * movementSpeed * Time.deltaTime;
        float yMovement = _verticalVelocity * Time.deltaTime;
        
        Vector3 movement = new Vector3(xMovement, yMovement, zMovement);
        _characterController.Move(movement);
    }

    private void Jump(InputAction.CallbackContext context)
    {
        Debug.Log("Jump pressed!");
        if (_characterController.isGrounded)
        {
            _verticalVelocity += jumpVelocity;
        }
    }
}
