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
        public Vector3 HorizontalVelocity;
        public float VerticalVelocity;

        public ReconcileData(Vector3 position, Vector3 horizontalVelocity, float verticalVelocity)
        {
            Position = position;
            HorizontalVelocity = horizontalVelocity;
            VerticalVelocity = verticalVelocity;
            _tick = 0;
        }

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    [Header("Horizontal Movement")]
    [SerializeField]
    [Tooltip("The maximum speed the player can move on the ground.")]
    private float maxGroundSpeed = 5f;
    [SerializeField]
    [Tooltip("The ammount the player decelerates when on the ground.")]
    private float groundDeceleration = 20f;
    [SerializeField]
    [Tooltip("Effects how much speed the player will gain when air strafing.")]
    private float airStrafeIntensity = 0.5f;
    [SerializeField]
    [Tooltip("Maximum acceleration of the player.")]
    private float maxAcceleration = 50f;

    [Header("Vertical Movement")]
    [SerializeField]
    [Tooltip("The upward velocity that will be set when the character jumps.")]
    private float jumpVelocity = 5f;
    [SerializeField]
    [Tooltip("The upward velocity that will be set when the character is on the ground. This must be configured to prevent the character controller from returning incorrect isGrounded values.")]
    private float groundedVerticalVelocity = -0.1f;
    [SerializeField]
    [Tooltip("The force of gravity on the player.")]
    private float gravity = -9.81f;

    private Queue<MoveData> _movesSinceLastTick;
    private bool _reconciledThisTick;

    private Vector3 _horizontalVelocity;
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

        Vector3 wishDir = new(md.Horizontal, 0f, md.Vertical);
        wishDir.Normalize();
        wishDir = Quaternion.Euler(0, md.CurrentRotation, 0) * wishDir;

        if (_characterController.isGrounded) UpdateHorizontalGroundVelocity(wishDir, md.Delta);
        else UpdateHorizontalAirVelocity(wishDir, md.Delta);

        Velocity = new Vector3(_horizontalVelocity.x, _verticalVelocity, _horizontalVelocity.z);
        _characterController.Move(Velocity * md.Delta);
    }

    private void UpdateHorizontalGroundVelocity(Vector3 wishDir, float deltaTime)
    {
        ApplyDeceleration(deltaTime);

        float projectedSpeed = Vector3.Dot(_horizontalVelocity, wishDir);
        float addSpeed = Mathf.Clamp(maxGroundSpeed - projectedSpeed, 0, maxAcceleration * deltaTime);

        _horizontalVelocity += addSpeed * wishDir;

        if (_horizontalVelocity.magnitude > maxGroundSpeed)
            _horizontalVelocity *= maxGroundSpeed / _horizontalVelocity.magnitude;
    }

    private void UpdateHorizontalAirVelocity(Vector3 wishDir, float deltaTime)
    {
        float projectedSpeed = Vector3.Dot(_horizontalVelocity, wishDir);
        float addSpeed = Mathf.Clamp(airStrafeIntensity - projectedSpeed, 0, maxAcceleration * deltaTime);

        _horizontalVelocity += addSpeed * wishDir;
    }

    private void ApplyDeceleration(float deltaTime)
    {
        float speed = _horizontalVelocity.magnitude;
        if (speed == 0) return;

        float newSpeed = speed - groundDeceleration * deltaTime;
        newSpeed = Mathf.Max(newSpeed, 0);

        _horizontalVelocity *= newSpeed / speed;
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
        ReconcileData rd = new ReconcileData(transform.position, _horizontalVelocity, _verticalVelocity);
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
        _horizontalVelocity = rd.HorizontalVelocity;
        _verticalVelocity = rd.VerticalVelocity;

        _reconciledThisTick = true;
    }

    private void Jump()
    {
        if (_characterController.isGrounded)
        {
            _verticalVelocity = jumpVelocity;
        }
    }

    private void ApplyGravity(float deltaTime)
    {
        _verticalVelocity += gravity * deltaTime;

        if (_characterController.isGrounded && _verticalVelocity <= groundedVerticalVelocity)
        {
            _verticalVelocity = groundedVerticalVelocity;
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