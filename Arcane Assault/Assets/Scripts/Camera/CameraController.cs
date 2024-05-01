using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Transform _anchorPoint;
    
    private Vector3 _targetPosition;
    private Vector3 _prevTargetPosition;
    private float _lerpProgress;

    private bool _updateDisabled;

    public float TickDelta { private get; set; }

    private void Update()
    {
        if (_updateDisabled || !_anchorPoint || TickDelta == 0f) return;

        if (_targetPosition != _anchorPoint.position)
        {
            _lerpProgress = 0f;
            _prevTargetPosition = _targetPosition;
            _targetPosition = _anchorPoint.position;
        }

        _lerpProgress += Time.deltaTime / TickDelta;
        transform.position = Vector3.Lerp(_prevTargetPosition, _targetPosition, _lerpProgress);

        transform.rotation = _anchorPoint.rotation;
    }

    public void SnapCameraToAnchor(Transform newAnchor)
    {
        _anchorPoint = newAnchor;

        _targetPosition = _anchorPoint.position;
        _prevTargetPosition = _targetPosition;
        transform.position = _targetPosition;

        transform.rotation = _anchorPoint.rotation;
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
