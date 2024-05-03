using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class OwnerObjectModifier : NetworkBehaviour
{
    [SerializeField] private List<GameObject> toggleActiveOnStart;
    [SerializeField] private List<GameObject> hideOnStart;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner) return;
        
        foreach (GameObject o in toggleActiveOnStart)
        {
            o.SetActive(!o.activeSelf);
        }

        foreach (GameObject o in hideOnStart)
        {
            o.layer = LayerMask.NameToLayer("Hidden");
        }
    }
}
