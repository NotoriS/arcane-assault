using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpellArc : MonoBehaviour
{
    [SerializeField] private float arcRange;

    private void Awake()
    {
        iTween.PunchPosition(gameObject, new Vector3(Random.Range(-arcRange, arcRange), Random.Range(-arcRange, arcRange), 0f), 5f);
    }
}
