using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private string targetTag;
    [SerializeField] private float damage;

    private void OnCollisionEnter2D(Collision2D other)
    {
        // Обрабатываем столкновения ТОЛЬКО на сервере
        if (!IsServer) return;
        
        if (other.gameObject.CompareTag(targetTag))
        {
            Debug.Log("[Server] Bullet hit " + other.gameObject.name);
            var health = other.gameObject.GetComponent<Health>();
            if (health != null)
            {
                // Меняем здоровье напрямую (мы на сервере)
                health.Damage(damage); // метод внутри Health должен работать на сервере
            }
        }
        
        // Удаляем пулю корректно через Netcode
        var netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned) netObj.Despawn();
        else Destroy(gameObject);
    }
}

