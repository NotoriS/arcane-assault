using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttack : MonoBehaviour
{
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform cameraTrasform;

    private PlayerInput _playerInput;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (_playerInput.BasicAttacked)
        {
            GameObject spell = Instantiate(spellPrefab, spawnPoint.position, cameraTrasform.rotation);
            spell.GetComponent<SpellMovement>().Initialize(cameraTrasform.position);
        }
    }
}
