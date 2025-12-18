using System;
using Unity.Netcode;
using UnityEngine;

public class Attack : NetworkBehaviour
{
    [SerializeField] private string targetTag;
    [SerializeField] private int damage = 10;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            other.GetComponent<Health>().Damage(damage);
        }
    }
}
