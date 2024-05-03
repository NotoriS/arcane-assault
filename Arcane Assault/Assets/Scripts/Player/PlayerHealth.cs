using System;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private TextMeshProUGUI healthText;

    [SerializeField] private GameObject playerRig;
    [SerializeField] private GameObject playerRenderer;
    [SerializeField] private Transform deathCamAnchorPoint;
    [SerializeField] private float deathCamTransitionTime;

    private readonly SyncVar<int> _currentHealth = new();
    private readonly List<Collider> _ragdollParts = new();
    
    public event Action OnPlayerDeath;

    private void Awake()
    {
        SetRagdollParts();
    }

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
        
        healthText.text = next.ToString();

        if (next <= 0) Kill();
    }

    private void Kill()
    {
        OnPlayerDeath?.Invoke();
        playerRenderer.layer = LayerMask.NameToLayer("Default");
        EnableRagdoll();
        
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

    private void SetRagdollParts()
    {
        Collider[] colliders = playerRig.GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders)
        {
            c.isTrigger = true;
            c.attachedRigidbody.isKinematic = true;
            _ragdollParts.Add(c);
        }
    }

    private void EnableRagdoll()
    {
        gameObject.GetComponent<CharacterController>().enabled = false;
        foreach (Collider c in _ragdollParts)
        {
            c.isTrigger = false;
            c.attachedRigidbody.isKinematic = false;
            c.attachedRigidbody.velocity = GetComponent<SynchronizedPlayerMovement>().Velocity;
        }
    }
}
