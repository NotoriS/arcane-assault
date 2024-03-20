using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class PlayerMovement : NetworkBehaviour
{
    private PlayerInputActions _playerInputActions;
    private CharacterController _characterController;
    private Camera _playerCamera;

    [SerializeField] private float movementSpeed = 2.5f;
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private float jumpVelocity = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckPadding = 0.01f;

    private float _verticalVelocity;

    private float _horizontalRotation;
    private float _verticalRotation;

    private class PlayerPositionState
    {
        public Vector3 Position { get; set; }
        public float HorizontalRotation { get; set; }
        public float VerticalRotation { get; set; }

        public PlayerPositionState(Vector3 position, float horizontalRotation, float verticalRotation)
        {
            Position = position;
            HorizontalRotation = horizontalRotation;
            VerticalRotation = verticalRotation;
        }
    }

    private readonly List<PlayerPositionState> _clientSidePredictionHistory = new();

    private void Awake()
    {
        _characterController = gameObject.GetComponent<CharacterController>();
        _playerCamera = gameObject.GetComponentInChildren<Camera>(true);

        _playerInputActions = new PlayerInputActions();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        OnEnable();
        if (IsOwner)
            gameObject.GetComponent<NetworkTransform>().enabled = false;
    }

    private void OnEnable()
    {
        if (!IsOwner) return;
        
        Cursor.lockState = CursorLockMode.Locked;
        _playerInputActions.Player.Enable();
    }

    private void OnDisable()
    {
        if (!IsOwner) return;
        
        _playerInputActions.Player.Disable();
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update()
    {
        if (IsOwner && !IsServer) // Owner client
        {
            Vector2 movementInput = _playerInputActions.Player.PlayerMovement.ReadValue<Vector2>();
            Vector2 cameraInput = _playerInputActions.Player.CameraMovement.ReadValue<Vector2>();
            bool jumpInput = _playerInputActions.Player.Jump.WasPerformedThisFrame();
        
            Move(Time.deltaTime, movementInput, cameraInput, jumpInput);
            PlayerPositionState currentState =
                new PlayerPositionState(transform.position, _horizontalRotation, _verticalRotation);
            _clientSidePredictionHistory.Add(currentState);
        
            MoveServerRpc(Time.deltaTime, movementInput, cameraInput, jumpInput);
        }
    }

    private void Move(float deltaTime, Vector2 movementInput, Vector2 cameraInput, bool jumpInput)
    {
        HalfApplyGravity(deltaTime);
        MoveCamera(cameraInput);
        MovePlayer(deltaTime, movementInput, jumpInput);
        HalfApplyGravity(deltaTime);
    }
    
    [ServerRpc]
    private void MoveServerRpc(float deltaTime, Vector2 movementInput, Vector2 cameraInput, bool jumpInput)
    {
        Move(deltaTime, movementInput, cameraInput, jumpInput);
        MoveClientRpc(transform.position, _horizontalRotation, _verticalRotation);
    }
    
    [ClientRpc]
    private void MoveClientRpc(Vector3 position, float horizontalRotation, float verticalRotation)
    {
        if (IsOwner)
        {
            if (_clientSidePredictionHistory.Count <= 0) return;
            PlayerPositionState prediction = _clientSidePredictionHistory[0];
            _clientSidePredictionHistory.RemoveAt(0);

            Vector3 positionPredictionError = position - prediction.Position;
            transform.position += positionPredictionError;

            float hrPredictionError = horizontalRotation - prediction.HorizontalRotation;
            _horizontalRotation += hrPredictionError;

            float vrPredictionError = verticalRotation - prediction.VerticalRotation;
            _verticalRotation += vrPredictionError;
            
            foreach (PlayerPositionState p in _clientSidePredictionHistory)
            {
                p.Position += positionPredictionError;
                p.HorizontalRotation += hrPredictionError;
                p.VerticalRotation += vrPredictionError;
            }
            
            transform.rotation = Quaternion.Euler(0, _horizontalRotation, 0);
            _playerCamera.transform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
        }
    }

    // Must be called before and after updating position.
    private void HalfApplyGravity(float deltaTime)
    {
        _verticalVelocity += gravity * deltaTime * 0.5f;
        if (_characterController.isGrounded && _verticalVelocity <= 0)
        {
            _verticalVelocity = 0;
        }
    }

    private void MoveCamera(Vector2 cameraInput)
    {
        _horizontalRotation += cameraInput.x * mouseSensitivity;
        _verticalRotation -= cameraInput.y * mouseSensitivity;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -90f, 90f);
        
        transform.rotation = Quaternion.Euler(0, _horizontalRotation, 0);
        _playerCamera.transform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
    }
    
    private void MovePlayer(float deltaTime, Vector2 movementInput, bool jumpInput)
    {
        if (jumpInput) Jump();
        
        float zMovement = movementInput.y * movementSpeed * deltaTime;
        float xMovement = movementInput.x * movementSpeed * deltaTime;
        float yMovement = _verticalVelocity * deltaTime;
        
        Vector3 movement = new Vector3(xMovement, yMovement, zMovement);
        movement = transform.TransformDirection(movement);
        
        _characterController.Move(movement);
    }
    
    private void Jump()
    {
        if (IsGrounded())
        {
            _verticalVelocity += jumpVelocity;
        }
    }

    private bool IsGrounded()
    {
        Vector3 castOrigin = transform.position + new Vector3(0, 1, 0);
        float maxDistance = 1f + groundCheckPadding;
        return Physics.Raycast(castOrigin, -transform.up, maxDistance);
    }
}
