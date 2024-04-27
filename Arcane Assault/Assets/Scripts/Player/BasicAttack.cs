using FishNet.Object;
using UnityEngine;

public class BasicAttack : NetworkBehaviour
{
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform cameraTransform;

    [SerializeField] private float arcRange;
    [SerializeField] private float arcTime;

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
        AttackVariance variance = new(Random.Range(-arcRange, arcRange), Random.Range(-arcRange, arcRange), arcTime);
        spell.GetComponent<SpellMovement>().Initialize(cameraTransform.position, variance);
        spell.GetComponentInChildren<SpellDamage>().Initialize(base.OwnerId);
        ServerFire(cameraTransform.position, cameraTransform.rotation, variance, TimeManager.Tick, base.OwnerId);
    }
    
    [ServerRpc]
    private void ServerFire(Vector3 camPosition, Quaternion direction, AttackVariance variance, uint tick, int shooterId)
    {
        if (!base.IsOwner)
        {
            GameObject spell = Instantiate(spellPrefab, spawnPoint.position, direction);
            float latency = (float)TimeManager.TimePassed(tick, false);
            latency = Mathf.Min(latency, MAXIMUM_LATENCY_COMPENSATION / 2f);
            spell.GetComponent<SpellMovement>().Initialize(camPosition, variance, latency);
            spell.GetComponentInChildren<SpellDamage>().Initialize(shooterId);
        }

        ObserversFire(camPosition, direction, variance, tick, shooterId);
    }

    [ObserversRpc(ExcludeOwner = true, ExcludeServer = true)]
    private void ObserversFire(Vector3 camPosition, Quaternion direction, AttackVariance variance, uint tick, int shooterId)
    {
        GameObject spell = Instantiate(spellPrefab, spawnPoint.position, direction);
        float latency = (float)TimeManager.TimePassed(tick, false);
        latency = Mathf.Min(latency, MAXIMUM_LATENCY_COMPENSATION);
        spell.GetComponent<SpellMovement>().Initialize(camPosition, variance, latency);
        spell.GetComponentInChildren<SpellDamage>().Initialize(shooterId);
    }
}
