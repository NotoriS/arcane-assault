using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;

    private int _currentHealth;
    
    void Start()
    {
        _currentHealth = maxHealth;
    }

    public void Damage(int amount)
    {
        _currentHealth -= amount;
    }
}
