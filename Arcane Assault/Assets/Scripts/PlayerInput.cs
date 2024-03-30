using FishNet.Object;
using UnityEngine;

public class PlayerInput : NetworkBehaviour
{
    public Vector2 MovementInput { get; private set; }
    public Vector2 CameraInput { get; private set; }
    public bool JumpQueued { get; set; }

    private PlayerInputActions _playerInputActions;

    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        OnEnable();
    }

    private void OnEnable()
    {
        if (!base.IsOwner) return;

        Cursor.lockState = CursorLockMode.Locked;
        _playerInputActions.Player.Enable();
    }

    private void OnDisable()
    {
        if (!base.IsOwner) return;

        _playerInputActions.Player.Disable();
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update()
    {
        if (!base.IsOwner) return;

        MovementInput = _playerInputActions.Player.PlayerMovement.ReadValue<Vector2>();
        CameraInput = _playerInputActions.Player.CameraMovement.ReadValue<Vector2>();
        JumpQueued |= _playerInputActions.Player.Jump.WasPerformedThisFrame();
    }
}
