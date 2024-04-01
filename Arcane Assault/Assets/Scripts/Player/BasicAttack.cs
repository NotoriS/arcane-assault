using FishNet.Object;
using UnityEngine;

public class BasicAttack : NetworkBehaviour
{
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform cameraTransform;

    private PlayerInput _playerInput;

    private const float MAXIMUM_LATENCY_COMPENSATION = 0.3f;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (_playerInput.BasicAttacked)
        {
            Fire();
        }
    }

    private void Fire()
    {
        GameObject spell = Instantiate(spellPrefab, spawnPoint.position, cameraTransform.rotation);
        spell.GetComponent<SpellMovement>().Initialize(cameraTransform.position);
        ServerFire(cameraTransform.position, cameraTransform.rotation, TimeManager.Tick);
    }
    
    [ServerRpc]
    private void ServerFire(Vector3 camPosition, Quaternion direction, uint tick)
    {
        if (!base.IsOwner)
        {
            GameObject spell = Instantiate(spellPrefab, spawnPoint.position, direction);
            float latency = (float)TimeManager.TimePassed(tick, false);
            latency = Mathf.Min(latency, MAXIMUM_LATENCY_COMPENSATION / 2f);
            spell.GetComponent<SpellMovement>().Initialize(camPosition, latency);
        }

        ObserversFire(camPosition, direction, tick);
    }

    [ObserversRpc(ExcludeOwner = true, ExcludeServer = true)]
    private void ObserversFire(Vector3 camPosition, Quaternion direction, uint tick)
    {
        GameObject spell = Instantiate(spellPrefab, spawnPoint.position, direction);
        float latency = (float)TimeManager.TimePassed(tick, false);
        latency = Mathf.Min(latency, MAXIMUM_LATENCY_COMPENSATION);
        spell.GetComponent<SpellMovement>().Initialize(camPosition, latency);
    }
}
