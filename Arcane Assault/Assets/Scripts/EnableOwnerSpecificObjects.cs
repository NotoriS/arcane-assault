using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnableOwnerSpecificObjects : NetworkBehaviour
{
    [SerializeField] private List<GameObject> objects;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            objects.ForEach(e => e.SetActive(true));
        }
    }
}
