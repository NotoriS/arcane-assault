using System.Collections.Generic;
using System.Linq;
using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

public class SynchronizedPlayerMovement : NetworkBehaviour
{
    public struct MoveData
    {
        public float Horizontal;
        public float Vertical;
        public bool JumpPressed;
        public float CurrentRotation;
        public float Delta;

        public MoveData(float horizontal, float vertical, bool jumpPressed, float currentRotation, float delta)
        {
            Horizontal = horizontal;
            Vertical = vertical;
            JumpPressed = jumpPressed;
            CurrentRotation = currentRotation;
            Delta = delta;
        }
    }

    public struct ReplicateData : IReplicateData
    {
        public MoveData[] Moves;

        public ReplicateData(IEnumerable<MoveData> moves)
        {
            Moves = moves.ToArray();
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

    private Queue<MoveData> _movesSinceLastTick;
    private bool _reconciledThisTick;
    
    private float _verticalVelocity;
    private bool _positionLocked;

    private CharacterController _characterController;
    private PlayerInput _playerInput;
    private PlayerLook _playerLook;
    
    public Vector3 Velocity { get; private set; }

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();
        _playerLook = GetComponent<PlayerLook>();
        
        GetComponent<PlayerHealth>().OnPlayerDeath += LockPosition;

        _movesSinceLastTick = new Queue<MoveData>();

        _positionLocked = true;
    }

    public override void OnStartNetwork()
    {
        SubscribeToTickEvents();
        _positionLocked = false;
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

    private void Update()
    {
        if (_positionLocked || !base.IsOwner) return;
        
        MoveData md = BuildMoveData();
        Move(md);
        _movesSinceLastTick.Enqueue(md);
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

        return new MoveData(horizontal, vertical, jumpPressed, currentRotation, Time.deltaTime);
    }

    private void Move(MoveData md)
    {
        if (md.JumpPressed) Jump();
        ApplyGravity(md.Delta);

        Vector3 move = new Vector3(md.Horizontal, 0f, md.Vertical).normalized * moveRate + new Vector3(0f, _verticalVelocity, 0f);
        move = Quaternion.Euler(0, md.CurrentRotation, 0) * move;

        _characterController.Move(move * md.Delta);
        Velocity = move;
    }

    private void TimeManager_OnTick()
    {
        if (_positionLocked) return;
        ReplicatedMove(BuildReplicateData());
    }

    private void TimeManager_OnPostTick()
    {
        if (_positionLocked) return;
        CreateReconcile();
        _reconciledThisTick = false;
    }

    private ReplicateData BuildReplicateData()
    {
        if (!base.IsOwner)
            return default;

        ReplicateData rd = new(_movesSinceLastTick);
        _movesSinceLastTick = new Queue<MoveData>();
        return rd;
    }

    public override void CreateReconcile()
    {
        if (!base.IsServerInitialized) return;
        ReconcileData rd = new ReconcileData(transform.position, _verticalVelocity);
        Reconciliation(rd);
    }

    [Replicate]
    private void ReplicatedMove(ReplicateData rd, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
    {
        // Stops the client from moving a second time when no reconciliation has occured
        if (base.IsOwner && !_reconciledThisTick) return;
        
        if (rd.Moves == null) return;
        foreach (MoveData md in rd.Moves)
        {
            Move(md);
        }
    }

    [Reconcile]
    private void Reconciliation(ReconcileData rd, Channel channel = Channel.Unreliable)
    {
        transform.position = rd.Position;
        _verticalVelocity = rd.VerticalVelocity;

        _reconciledThisTick = true;
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
        return Physics.Raycast(castOrigin, -transform.up, maxDistance, LayerMask.NameToLayer("PlayerModel"));
    }

    private void ApplyGravity(float deltaTime)
    {
        float stillVelocity = _characterController.stepOffset / -2f / deltaTime;
        _verticalVelocity += Physics.gravity.y * deltaTime;

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
    
    [CustomComparer]
    public static bool CompareMoveDataArray(MoveData[] a, MoveData[] b)
    {
        bool aNull = (a is null);
        bool bNull = (b is null);
            
        if (aNull && bNull)
            return true;
            
        if (aNull != bNull)
            return false;
            
        if (a.Length != b.Length)
            return false;
            
        int length = a.Length;
        for (int i = 0; i < length; i++)
        {
            if (!a[i].Equals(b[i]))
                return false;
        }
            
        return true;
    }
}