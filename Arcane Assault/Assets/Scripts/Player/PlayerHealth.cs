using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;

    [SyncVar(Channel = Channel.Unreliable, SendRate = 0f, OnChange = nameof(OnCurrentHealthChanged))]
    private int _currentHealth;

    public override void OnStartServer()
    {
        _currentHealth = maxHealth;
    }

    public void Damage(int amount)
    {
        if (!base.IsServer) return;
        _currentHealth = Mathf.Max(_currentHealth - amount, 0);
    }

    private void OnCurrentHealthChanged(int prev, int next, bool asServer)
    {
        if (asServer && !base.IsServerOnly) return;
        Debug.Log($"Player #{base.OwnerId} Health: {_currentHealth}");
    }
}
