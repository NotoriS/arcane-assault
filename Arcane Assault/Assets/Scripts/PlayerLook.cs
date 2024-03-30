using FishNet.Object;
using UnityEngine;

public class PlayerLook : NetworkBehaviour
{
    [SerializeField] private float mouseSensitivity = 1f;

    public float Rotation { get; private set; }
    public float CameraPitch { get; private set; }

    private PlayerInput _playerInput;
    private Camera _playerCamera;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _playerCamera = GetComponentInChildren<Camera>(true);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner) enabled = false;
    }

    private void Update()
    {
        if (!base.IsOwner) return;

        Rotation += _playerInput.LookInput.x * mouseSensitivity;
        CameraPitch -= _playerInput.LookInput.y * mouseSensitivity;
        CameraPitch = Mathf.Clamp(CameraPitch, -90f, 90f);

        transform.rotation = Quaternion.Euler(0, Rotation, 0);
        _playerCamera.transform.localRotation = Quaternion.Euler(CameraPitch, 0, 0);
    }
}
