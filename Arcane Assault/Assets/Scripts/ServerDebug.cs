using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ServerDebug : MonoBehaviour
{
    private PlayerInputActions _playerInputActions;
    
    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        _playerInputActions.ServerDebug.Enable();
    }

    private void OnEnable()
    {
        _playerInputActions.ServerDebug.StartServer.performed += StartServer;
        _playerInputActions.ServerDebug.StartClient.performed += StartClient;
    }

    private void OnDisable()
    {
        _playerInputActions.ServerDebug.StartClient.performed -= StartClient;
        _playerInputActions.ServerDebug.StartServer.performed -= StartServer;
    }

    private static void StartServer(InputAction.CallbackContext context)
    {
        Debug.Log("Server Starting");
        NetworkManager.Singleton.StartServer();
    }
    
    private static void StartClient(InputAction.CallbackContext context)
    {
        Debug.Log("Client Starting");
        NetworkManager.Singleton.StartClient();
    }
}
