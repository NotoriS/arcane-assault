using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class ToggleOwnerSpecificObjects : NetworkBehaviour
{
    [SerializeField] private List<GameObject> objects;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner) return;
        foreach (GameObject o in objects)
        {
            o.SetActive(!o.activeSelf);
        }
    }
}
