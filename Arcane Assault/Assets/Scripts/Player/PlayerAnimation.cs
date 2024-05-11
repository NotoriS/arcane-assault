using System;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private float movementTransitionAcceleration;

    private Vector3 _currentAnimatedVelocity;
    
    private Animator _animator;
    private SynchronizedPlayerMovement _playerMovement;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _playerMovement = GetComponent<SynchronizedPlayerMovement>();
    }

    private void Update()
    {
        Vector3 inputVelocity = Quaternion.Inverse(transform.rotation) * _playerMovement.Velocity;

        Vector3 diff = inputVelocity - _currentAnimatedVelocity;
        Vector3 transition = diff.normalized * (movementTransitionAcceleration * Time.deltaTime);
        transition = diff.magnitude < transition.magnitude ? diff : transition;
        _currentAnimatedVelocity += transition;
        
        _animator.SetFloat("VelocityZ", _currentAnimatedVelocity.z);
        _animator.SetFloat("VelocityX", _currentAnimatedVelocity.x);
    }
}
