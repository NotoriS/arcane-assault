using Unity.Netcode;

public class ClientSelfDisable : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsClient)
        {
            gameObject.SetActive(false);
        }
    }
}
