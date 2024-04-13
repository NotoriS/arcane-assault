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
        spell.GetComponent<SpellDamage>().Initialize(base.OwnerId);
        ServerFire(cameraTransform.position, cameraTransform.rotation, TimeManager.Tick, base.OwnerId);
    }
    
    [ServerRpc]
    private void ServerFire(Vector3 camPosition, Quaternion direction, uint tick, int shooterId)
    {
        if (!base.IsOwner)
        {
            GameObject spell = Instantiate(spellPrefab, spawnPoint.position, direction);
            float latency = (float)TimeManager.TimePassed(tick, false);
            latency = Mathf.Min(latency, MAXIMUM_LATENCY_COMPENSATION / 2f);
            spell.GetComponent<SpellMovement>().Initialize(camPosition, latency);
            spell.GetComponent<SpellDamage>().Initialize(shooterId);
        }

        ObserversFire(camPosition, direction, tick, shooterId);
    }

    [ObserversRpc(ExcludeOwner = true, ExcludeServer = true)]
    private void ObserversFire(Vector3 camPosition, Quaternion direction, uint tick, int shooterId)
    {
        GameObject spell = Instantiate(spellPrefab, spawnPoint.position, direction);
        float latency = (float)TimeManager.TimePassed(tick, false);
        latency = Mathf.Min(latency, MAXIMUM_LATENCY_COMPENSATION);
        spell.GetComponent<SpellMovement>().Initialize(camPosition, latency);
        spell.GetComponent<SpellDamage>().Initialize(shooterId);
    }
}
