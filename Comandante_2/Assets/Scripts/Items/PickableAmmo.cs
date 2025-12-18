using UnityEngine;

public class PickableAmmo : MonoBehaviour
{
    [SerializeField] private int ammoCount = 10;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<AmmoController>().AddAmmo(ammoCount);
            Destroy(gameObject);
        }
    }
}
