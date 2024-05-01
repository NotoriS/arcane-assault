using UnityEngine;

public class CameraAnchorFocus : MonoBehaviour
{
    [SerializeField] public Transform focusPoint;
    
    private void Update()
    {
        if (!focusPoint) return;
        
        transform.LookAt(focusPoint);
    }
}
