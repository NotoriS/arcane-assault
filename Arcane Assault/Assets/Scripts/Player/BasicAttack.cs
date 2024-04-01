using FishNet.Object;
using UnityEngine;
using UnityEngine.Serialization;

public class BasicAttack : NetworkBehaviour
{
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform cameraTransform;

    private PlayerInput _playerInput;

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
            spell.GetComponent<SpellMovement>().Initialize(camPosition, latency);
        }

        ObserversFire(camPosition, direction, tick);
    }

    [ObserversRpc(ExcludeOwner = true, ExcludeServer = true)]
    private void ObserversFire(Vector3 camPosition, Quaternion direction, uint tick)
    {
        GameObject spell = Instantiate(spellPrefab, spawnPoint.position, direction);
        float latency = (float)TimeManager.TimePassed(tick, false);
        spell.GetComponent<SpellMovement>().Initialize(camPosition, latency);
    }
}
