using FishNet.Object;
using UnityEngine;

public class SpellDamage : MonoBehaviour
{
    [SerializeField] private int spellDamage;

    private int _spawnerId;

    public void Initialize(int spawnerId)
    {
        _spawnerId = spawnerId;
    }

    private void OnTriggerEnter(Collider collider)
    {
        NetworkObject networkObject = collider.gameObject.GetComponent<NetworkObject>();
        //if (networkObject != null)
        //{
        //    Debug.Log($"{_spawnerId} : {networkObject.OwnerId}");
        //}
        //else 
        //{ 
        //    Debug.Log("null"); 
        //}
        if (networkObject != null && networkObject.OwnerId == _spawnerId) return; // Hit self

        IDamageable damageable = collider.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.Damage(spellDamage);
        }

        Debug.Log("Destroying");
        Destroy(gameObject);
    }
}
