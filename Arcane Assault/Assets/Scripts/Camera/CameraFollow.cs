using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Vector3 _targetPosition;
    private Vector3 _prevTargetPosition;
    private float _lerpProgress;

    public float TickDelta { private get; set; }

    public Transform AnchorPoint { private get; set; }


    private void Update()
    {
        if (AnchorPoint == null || TickDelta == 0f) return;

        if (_targetPosition != AnchorPoint.position)
        {
            _lerpProgress = 0f;
            _prevTargetPosition = _targetPosition;
            _targetPosition = AnchorPoint.position;
        }

        _lerpProgress += Time.deltaTime / TickDelta;
        transform.position = Vector3.Lerp(_prevTargetPosition, _targetPosition, _lerpProgress);

        transform.rotation = AnchorPoint.rotation;
    }

    public void SnapCameraToAnchor()
    {
        if (AnchorPoint == null) return;

        _targetPosition = AnchorPoint.position;
        _prevTargetPosition = _targetPosition;
        transform.position = _targetPosition;

        transform.rotation = AnchorPoint.rotation;
    }
}
