using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Transform _anchorPoint;
    
    private Vector3 _targetPosition;
    private Vector3 _prevTargetPosition;
    private float _lerpProgress;

    public float TickDelta { private get; set; }

    private void Update()
    {
        if (!_anchorPoint || TickDelta == 0f) return;

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

    public void SnapCameraToAnchor(Transform anchor)
    {
        _anchorPoint = anchor;

        _targetPosition = _anchorPoint.position;
        _prevTargetPosition = _targetPosition;
        transform.position = _targetPosition;

        transform.rotation = _anchorPoint.rotation;
    }
}
