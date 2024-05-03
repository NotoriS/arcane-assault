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

    private void OnTriggerEnter(Collider c)
    {
        NetworkObject networkObject = c.transform.root.gameObject.GetComponent<NetworkObject>();
        if (networkObject != null && networkObject.OwnerId == _spawnerId) return; // Hit self

        IDamageable damageable = c.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.Damage(spellDamage);
        }

        Destroy(transform.parent.gameObject);
    }
}
