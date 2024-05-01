using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private TextMeshProUGUI healthText;

    [SerializeField] private GameObject playerModel;
    [SerializeField] private Transform deathCamAnchorPoint;
    [SerializeField] private float deathCamTransitionTime;

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

        if (next <= 0) Kill();
    }

    private void Kill()
    {
        gameObject.GetComponent<CharacterController>().enabled = false;
        gameObject.GetComponent<PlayerInput>().enabled = false;
        
        playerModel.SetActive(true);
        // TODO: Ragdoll character model
        
        if (!base.IsOwner) return;
        
        GameObject cameraObj = GameObject.FindWithTag("MainCamera");
        if (!cameraObj) Debug.LogError("Unable to find main camera object.");
        if (cameraObj.TryGetComponent(out CameraController camFollow))
        {
            camFollow.LerpCameraToAnchor(deathCamAnchorPoint, deathCamTransitionTime);
        }
        else
        {
            Debug.LogError("Unable to find CameraFollow component on main camera.");
        }
    }
}
