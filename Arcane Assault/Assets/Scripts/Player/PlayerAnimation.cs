using System;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
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
        _animator.SetFloat("VelocityZ", inputVelocity.z);
        _animator.SetFloat("VelocityX", inputVelocity.x);
    }
}
