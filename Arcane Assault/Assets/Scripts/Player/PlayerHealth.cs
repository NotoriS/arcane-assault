using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private TextMeshProUGUI healthText;

    private readonly SyncVar<int> _currentHealth = new();

    public override void OnStartNetwork()
    {
        _currentHealth.Value = maxHealth;
        _currentHealth.OnChange += OnCurrentHealthChanged;
    }

    public void Damage(int amount)
    {
        if (!base.IsServerInitialized) return;
        _currentHealth.Value = Mathf.Max(_currentHealth.Value - amount, 0);
    }

    private void OnCurrentHealthChanged(int prev, int next, bool asServer)
    {
        if (asServer && !base.IsServerOnlyInitialized) return;
        Debug.Log($"Player #{base.OwnerId} Health: {_currentHealth.Value}");
        healthText.text = next.ToString();
    }
}
