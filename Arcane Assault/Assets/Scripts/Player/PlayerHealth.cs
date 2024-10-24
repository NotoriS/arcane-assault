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

    [SerializeField] private GameObject ragdoll;
    [SerializeField] private GameObject playerModel;
    [SerializeField] private List<GameObject> playerRenderers;
    [SerializeField] private Transform deathCamAnchorPoint;
    [SerializeField] private float deathCamTransitionTime;

    private readonly SyncVar<int> _currentHealth = new();
    private readonly List<Collider> _ragdollParts = new();
    
    public event Action OnPlayerDeath;

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
        foreach (GameObject playerRenderer in playerRenderers)
        {
            playerRenderer.layer = LayerMask.NameToLayer("Default");
        }
        gameObject.GetComponent<CharacterController>().enabled = false;
        playerModel.SetActive(false);
        SpawnRagdoll();
        
        if (!base.IsOwner) return;
        CameraManager.Instance.MainCameraController.LerpCameraToAnchor(deathCamAnchorPoint, deathCamTransitionTime);
    }

    private void SpawnRagdoll()
    {
        GameObject spawnedRagdoll = Instantiate(ragdoll, transform.position, transform.rotation);
        CopyAllChildTranforms(playerModel.transform, spawnedRagdoll.transform);

        Vector3 currentPlayerVelocity = GetComponent<SynchronizedPlayerMovement>().Velocity;
        foreach (Rigidbody rb in spawnedRagdoll.GetComponentsInChildren<Rigidbody>())
        {
            rb.velocity = currentPlayerVelocity;
        }
    }

    public static void CopyAllChildTranforms(Transform original, Transform copy)
    {
        if (original.childCount < copy.childCount)
        {
            Debug.LogError("Child transforms cannot be copied to objects with more children than the original.");
            return;
        }

        copy.position = original.position;
        copy.rotation = original.rotation;

        for (int i = 0; i < copy.childCount; i++)
        {
            Transform originalChild = original.GetChild(i);
            Transform copyChild = copy.GetChild(i);

            if (originalChild.name != copyChild.name)
            {
                Debug.LogWarning($"A transform is being copied to an object with a different name. This may be a mistake. ({originalChild.name} to {copyChild.name})");
            }

            CopyAllChildTranforms(originalChild, copyChild);
        }
    }
}
