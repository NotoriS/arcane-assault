using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

public class SynchronizedPlayerMovement : NetworkBehaviour
{
    public struct MoveData : IReplicateData
    {
        public float Horizontal;
        public float Vertical;
        public bool JumpPressed;
        public float CurrentRotation;

        public MoveData(float horizontal, float vertical, bool jumpPressed, float currentRotation)
        {
            Horizontal = horizontal;
            Vertical = vertical;
            JumpPressed = jumpPressed;
            CurrentRotation = currentRotation;
            _tick = 0;
        }

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }
    public struct ReconcileData : IReconcileData
    {
        public Vector3 Position;
        public float VerticalVelocity;

        public ReconcileData(Vector3 position, float verticalVelocity)
        {
            Position = position;
            VerticalVelocity = verticalVelocity;
            _tick = 0;
        }

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    [SerializeField] private float moveRate = 5f;
    [SerializeField] private float jumpVelocity = 5f;
    [SerializeField] private float groundCheckPadding = 0.01f;

    private float _verticalVelocity;

    private bool _positionLocked;

    private CharacterController _characterController;
    private PlayerInput _playerInput;
    private PlayerLook _playerLook;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();
        _playerLook = GetComponent<PlayerLook>();
        
        GetComponent<PlayerHealth>().OnPlayerDeath += LockPosition;
    }

    public override void OnStartNetwork()
    {
        SubscribeToTickEvents();
    }

    public override void OnStopNetwork()
    {
        UnsubscribeFromTickEvents();
    }

    private void SubscribeToTickEvents()
    {
        base.TimeManager.OnTick += TimeManager_OnTick;
        base.TimeManager.OnPostTick += TimeManager_OnPostTick;
    }

    private void UnsubscribeFromTickEvents()
    {
        base.TimeManager.OnTick -= TimeManager_OnTick;
        base.TimeManager.OnPostTick -= TimeManager_OnPostTick;
    }

    private void TimeManager_OnTick()
    {
        Move(BuildMoveData());
    }

    private void TimeManager_OnPostTick()
    {
        CreateReconcile();
    }

    private MoveData BuildMoveData()
    {
        if (!base.IsOwner)
            return default;

        float horizontal = _playerInput.MovementInput.x;
        float vertical = _playerInput.MovementInput.y;

        bool jumpPressed = _playerInput.JumpQueued;
        _playerInput.JumpQueued = false;

        float currentRotation = _playerLook.Rotation;

        return new MoveData(horizontal, vertical, jumpPressed, currentRotation);
    }

    public override void CreateReconcile()
    {
        if (base.IsServerInitialized)
        {
            ReconcileData rd = new ReconcileData(transform.position, _verticalVelocity);
            Reconciliation(rd);
        }
    }

    [Replicate]
    private void Move(MoveData md, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
    {
        if (_positionLocked) return; // Might cause rubber banding on death
        
        if (md.JumpPressed) Jump();
        ApplyGravity();

        Vector3 move = new Vector3(md.Horizontal, 0f, md.Vertical).normalized * moveRate + new Vector3(0f, _verticalVelocity, 0f);
        move = Quaternion.Euler(0, md.CurrentRotation, 0) * move;

        _characterController.Move(move * (float)base.TimeManager.TickDelta);
    }

    [Reconcile]
    private void Reconciliation(ReconcileData rd, Channel channel = Channel.Unreliable)
    {
        transform.position = rd.Position;
        _verticalVelocity = rd.VerticalVelocity;
    }

    private void Jump()
    {
        if (IsGrounded())
        {
            _verticalVelocity = jumpVelocity;
        }
    }

    private bool IsGrounded()
    {
        Vector3 castOrigin = transform.position + new Vector3(0, 1, 0);
        float maxDistance = 1f + groundCheckPadding;
        return Physics.Raycast(castOrigin, -transform.up, maxDistance);
    }

    private void ApplyGravity()
    {
        float stillVelocity = _characterController.stepOffset / -2f / (float)base.TimeManager.TickDelta;
        _verticalVelocity += Physics.gravity.y * (float)base.TimeManager.TickDelta;

        if (_characterController.isGrounded && _verticalVelocity <= stillVelocity)
        {
            _verticalVelocity = stillVelocity;
        }
    }

    public void LockPosition()
    {
        _positionLocked = true;
    }
    
    public void UnlockPosition()
    {
        _positionLocked = false;
    }
}