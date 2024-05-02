using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Transform _anchorPoint;
    private bool _updateDisabled;

    private void Update()
    {
        if (_updateDisabled || !_anchorPoint) return;

        transform.position = _anchorPoint.position;
        transform.rotation = _anchorPoint.rotation;
    }

    public void SnapCameraToAnchor(Transform newAnchor)
    {
        _anchorPoint = newAnchor;
    }

    public void LerpCameraToAnchor(Transform newAnchor, float lerpTime)
    {
        StartCoroutine(LerpCameraToAnchorCoroutine(newAnchor, lerpTime));
    }
    
    private IEnumerator LerpCameraToAnchorCoroutine(Transform newAnchor, float lerpTime)
    {
        _updateDisabled = true;
        
        Vector3 startPos = transform.position;
        Vector3 startForward = transform.forward;
        Vector3 startUp = transform.up;

        float timeElapsed = 0f;
        while (timeElapsed < lerpTime)
        {
            timeElapsed += Time.deltaTime;
            timeElapsed = Mathf.Min(timeElapsed, lerpTime);
            
            Vector3 lerpedPos = Vector3.Lerp(startPos, newAnchor.position, timeElapsed / lerpTime);
            Vector3 lerpedForward = Vector3.Lerp(startForward, newAnchor.forward, timeElapsed / lerpTime);
            Vector3 lerpedUp = Vector3.Lerp(startUp, newAnchor.up, timeElapsed / lerpTime);

            transform.position = lerpedPos;
            transform.rotation = Quaternion.LookRotation(lerpedForward, lerpedUp);

            yield return null;
        }

        SnapCameraToAnchor(newAnchor);
        _updateDisabled = false;
    }
}
