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

    [SerializeField] private float _movementSpeed = 2.5f;
    [SerializeField] private float _mouseSensitivity = 1f;
    [SerializeField] private float _jumpVelocity = 5f;

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
        
        MoveCamera();
        MovePlayer();
    }

    private void MoveCamera()
    {
        // TODO
    }
    
    private void MovePlayer()
    {
        Vector3 movementDirection = new Vector3(_movementInput.x, 0, _movementInput.y);
        _characterController.Move(movementDirection * (_movementSpeed * Time.deltaTime));
    }

    private void Jump(InputAction.CallbackContext context)
    {
        // TODO
    }
}
