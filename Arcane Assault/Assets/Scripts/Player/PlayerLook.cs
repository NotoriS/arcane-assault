using FishNet;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

public class PlayerLook : NetworkBehaviour
{
    [SerializeField] private float mouseSensitivity = 1f;

    public float Rotation { get; private set; }
    public float CameraPitch { get; private set; }

    private PlayerInput _playerInput;
    
    [SerializeField] private Transform playerCameraAnchor;

    private float _rotationSentLastTick;
    private float _cameraPitchSentLastTick;

    private void Awake()
    {
        InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
        _playerInput = GetComponent<PlayerInput>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner) return;
        CameraManager.Instance.MainCameraController.SnapCameraToAnchor(playerCameraAnchor);
    }

    private void TimeManager_OnTick()
    {
        if (!base.IsOwner) return;
        if (Rotation == _rotationSentLastTick && CameraPitch == _cameraPitchSentLastTick) return;

        _rotationSentLastTick = Rotation;
        _cameraPitchSentLastTick = CameraPitch;
        UpdateServerOrientation(Rotation, CameraPitch);
    }

    private void Update()
    {
        if (!base.IsOwner) return;

        Rotation += _playerInput.LookInput.x * mouseSensitivity;
        CameraPitch -= _playerInput.LookInput.y * mouseSensitivity;
        CameraPitch = Mathf.Clamp(CameraPitch, -90f, 90f);

        transform.rotation = Quaternion.Euler(0, Rotation, 0);
        playerCameraAnchor.localRotation = Quaternion.Euler(CameraPitch, 0, 0);
    }

    [ServerRpc]
    private void UpdateServerOrientation(float rotation, float cameraPitch, Channel channel = Channel.Unreliable)
    {
        UpdateObserverOrientation(rotation, cameraPitch);
        if (base.IsOwner) return;

        transform.rotation = Quaternion.Euler(0, rotation, 0);
        playerCameraAnchor.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
    }

    [ObserversRpc(ExcludeOwner = true, ExcludeServer = true)]
    private void UpdateObserverOrientation(float rotation, float cameraPitch, Channel channel = Channel.Unreliable)
    {
        transform.rotation = Quaternion.Euler(0, rotation, 0);
        playerCameraAnchor.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
    }
}
