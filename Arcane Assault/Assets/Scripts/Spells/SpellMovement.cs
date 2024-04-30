using UnityEngine;

public class SpellMovement : MonoBehaviour
{
    [SerializeField] private float maxLifetime = 10f;
    [SerializeField] private float forwardVelocity = 10f;
    [SerializeField] private float distanceBeforeConvergence = 5f;

    [SerializeField] private GameObject spellVisual;

    private Vector3 _moveDirection;
    private Vector3 _upDirection;

    private Vector3 _lerpStart;
    private Vector3 _lerpTarget;
    private Vector3 _lerpProgress;

    private float _timeAlive;
    
    private float _catchupTimeRemaining;
    private float _catchupTimeThisFrame;

    public void Initialize(Vector3 cameraPosition, AttackVariance variance = default, float latency = 0f)
    {
        _catchupTimeRemaining = latency;
        _moveDirection = transform.forward;
        _upDirection = transform.up;

        // Find point on the target line that is perpendicular to the camera and spawn point
        Vector3 camToSpawn = transform.position - cameraPosition;
        float angleFromTargetLine = Vector3.Angle(_moveDirection, camToSpawn) * Mathf.Deg2Rad;
        float cameraOffsetLength = camToSpawn.magnitude * Mathf.Cos(angleFromTargetLine);

        _lerpStart = transform.position;
        _lerpTarget = cameraPosition + _moveDirection * cameraOffsetLength;

        iTween.PunchPosition(spellVisual, new Vector3(variance.X, variance.Y, 0f), variance.Time);
    }

    private void Update()
    {
        UpdateCatchupTime();
        float delta = Time.deltaTime + _catchupTimeThisFrame;
        _timeAlive += delta;
        if (_timeAlive > maxLifetime) Destroy(gameObject);
        
        Vector3 horizontalMovement = CalculateShiftThisFrame();
        Vector3 forwardMovement = _moveDirection * (forwardVelocity * delta);
        Vector3 totalMoveThisFrame = horizontalMovement + forwardMovement;

        transform.rotation = Quaternion.LookRotation(totalMoveThisFrame, _upDirection);
        transform.position += totalMoveThisFrame;
    }

    private void UpdateCatchupTime()
    {
        if (_catchupTimeRemaining <= 0f)
        {
            _catchupTimeThisFrame = 0f;
            return;
        }
        
        float step = (_catchupTimeRemaining * 0.08f);
        _catchupTimeRemaining -= step;
            
        if (_catchupTimeRemaining <= (Time.deltaTime / 2f))
        {
            step += _catchupTimeRemaining;
            _catchupTimeRemaining = 0f;
        }
        _catchupTimeThisFrame = step;
    }

    // Determine the distance to shift toward the target line this frame
    private Vector3 CalculateShiftThisFrame() 
    {
        float timeBeforeConvergence = distanceBeforeConvergence / forwardVelocity;
        float lerpT = SmoothInterpolant(_timeAlive / timeBeforeConvergence);
        Vector3 newProgress = Vector3.Lerp(_lerpStart, _lerpTarget, lerpT) - _lerpStart;
        Vector3 shiftThisFrame = newProgress - _lerpProgress;
        _lerpProgress = newProgress;
        return shiftThisFrame;
    }

    // Uses a sigmoidal function to smooth the shift toward the target
    private static float SmoothInterpolant(float interpolent, bool regularized = true)
    {
        float regularizer = regularized ? SmoothInterpolant(1f, false) : 1f;
        return ((1f / (1f + Mathf.Exp(-5f * interpolent))) - 0.5f) * 2f / regularizer;
    }
}
