using FishNet.Object;
using UnityEngine;

public class PlayerInput : NetworkBehaviour
{
    private bool _controlsLocked;
    
    public Vector2 MovementInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool JumpQueued { get; set; }
    public bool BasicAttacked { get; private set; }

    private PlayerInputActions _playerInputActions;

    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        GetComponent<PlayerHealth>().OnPlayerDeath += LockControls;
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
        if (!base.IsOwner || _controlsLocked) return;

        MovementInput = _playerInputActions.Player.PlayerMovement.ReadValue<Vector2>();
        LookInput = _playerInputActions.Player.CameraMovement.ReadValue<Vector2>();
        JumpQueued |= _playerInputActions.Player.Jump.WasPerformedThisFrame();
        BasicAttacked = _playerInputActions.Player.BasicAttack.WasPerformedThisFrame();
    }

    public void LockControls()
    {
        _controlsLocked = true;

        MovementInput = Vector2.zero;
        LookInput = Vector2.zero;
        JumpQueued = false;
        BasicAttacked = false;
    }
    
    public void UnlockControls()
    {
        _controlsLocked = false;
    }
}
