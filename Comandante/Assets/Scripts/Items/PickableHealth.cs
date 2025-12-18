using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickableHealth : MonoBehaviour
{

    [SerializeField] private int heal;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Health>().Heal(heal);
            Destroy(gameObject);
        }
    }
}
