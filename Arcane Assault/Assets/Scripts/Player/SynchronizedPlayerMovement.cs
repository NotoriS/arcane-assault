using FishNet;
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

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }
    public struct ReconcileData : IReconcileData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float VerticalVelocity;
        public ReconcileData(Vector3 position, Quaternion rotation, float verticalVelocity)
        {
            Position = position;
            Rotation = rotation;
            VerticalVelocity = verticalVelocity;
            _tick = 0;
        }

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    [SerializeField] private float moveRate = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpVelocity = 5f;
    [SerializeField] private float groundCheckPadding = 0.01f;

    private float _verticalVelocity = 0;

    private CharacterController _characterController;
    private PlayerInput _playerInput;
    private PlayerLook _playerLook;

    private void Awake()
    {
        InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
        _characterController = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();
        _playerLook = GetComponent<PlayerLook>();
    }

    public override void OnStartClient()
    {
        _characterController.enabled = (base.IsServer || base.IsOwner);
    }

    private void OnDestroy()
    {
        if (InstanceFinder.TimeManager != null)
        {
            InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
        }
    }

    private void TimeManager_OnTick()
    {
        if (!base.IsOwner && !base.IsServer) return;

        HalfApplyGravity();
        if (base.IsOwner)
        {
            Reconciliation(default, false);
            CheckInput(out MoveData md);
            Move(md, false);
        }
        if (base.IsServer)
        {
            Move(default, true);
            ReconcileData rd = new ReconcileData(transform.position, transform.rotation, _verticalVelocity);
            Reconciliation(rd, true);
        }
        HalfApplyGravity();
    }

    private void CheckInput(out MoveData md)
    {
        md = default;

        float horizontal = _playerInput.MovementInput.x;
        float vertical = _playerInput.MovementInput.y;

        bool jumpPressed = _playerInput.JumpQueued;
        _playerInput.JumpQueued = false;
        
        md = new MoveData()
        {
            Horizontal = horizontal,
            Vertical = vertical,
            JumpPressed = jumpPressed,
            CurrentRotation = _playerLook.Rotation
        };
    }

    [Replicate]
    private void Move(MoveData md, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false)
    {
        if (md.JumpPressed) Jump();

        Vector3 move = new Vector3(md.Horizontal, 0f, md.Vertical).normalized * moveRate + new Vector3(0f, _verticalVelocity, 0f);
        move = Quaternion.Euler(0, md.CurrentRotation, 0) * move;

        _characterController.Move(move * (float)base.TimeManager.TickDelta);
    }

    [Reconcile]
    private void Reconciliation(ReconcileData rd, bool asServer, Channel channel = Channel.Unreliable)
    {
        transform.position = rd.Position;
        transform.rotation = rd.Rotation;
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

    // Must be called before and after updating position.
    private void HalfApplyGravity()
    {
        _verticalVelocity += gravity * (float)base.TimeManager.TickDelta * 0.5f;
        if (_characterController.isGrounded && _verticalVelocity <= 0)
        {
            _verticalVelocity = 0;
        }
    }
}