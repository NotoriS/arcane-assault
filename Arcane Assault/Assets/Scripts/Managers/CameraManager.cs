using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    public CameraController MainCameraController { get; private set; }

    protected override void OnAwake()
    {
        AssignMainCameraController();
    }

    private void AssignMainCameraController()
    {
        GameObject cameraObj = GameObject.FindWithTag("MainCamera");
        if (!cameraObj) Debug.LogError("[CameraManager] Unable to find main camera object.");
        if (cameraObj.TryGetComponent(out CameraController cameraController))
        {
            MainCameraController = cameraController;
        }
        else
        {
            Debug.LogError("[CameraManager] Unable to find CameraController component on main camera.");
        }
    }
}
