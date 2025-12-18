using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : MonoBehaviour
{
    [SerializeField] private float explosionPower = 5000;
    [SerializeField] private Animator animator;
    

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GetComponent<AudioSource>().PlayOneShot(GetComponent<AudioSource>().clip);
            other.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.up*explosionPower);
            other.gameObject.GetComponent<Health>().Damage(10000);
            animator.enabled = true;
            // Destroy(transform.Find("mine").gameObject);
        }
    }
}
