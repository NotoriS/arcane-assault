using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ToggleOwnerSpecificObjects : NetworkBehaviour
{
    [SerializeField] private List<GameObject> objects;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner) return; 
        foreach (GameObject o in objects)
        {
            o.SetActive(!o.activeSelf);
        }
    }
}
