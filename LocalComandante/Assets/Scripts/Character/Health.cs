using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    private StateHolder _stateHolder;

    private void Awake()
    {
        _stateHolder = GetComponent<StateHolder>();
    }

    // Вызывается локально или сервером — внутри проверяем IsServer
    public void Damage(float damage)
    {
        if (!IsServer)
        {
            // Если вызвали на клиенте — пересылаем запрос серверу (RequireOwnership = false позволяет этому клиенту послать запрос)
            DamageServerRpc(damage);
            return;
        }

        // Тут выполняется ТОЛЬКО на сервере
        _stateHolder.health.Value = Mathf.Max(0f, _stateHolder.health.Value - damage);
        Debug.Log($"[Server] Health now = {_stateHolder.health.Value} for {gameObject.name}");

        // Можно добавить логику смерти/уведомления
        if (_stateHolder.health.Value <= 0f)
        {
            // обработка смерти
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DamageServerRpc(float damage, ServerRpcParams rpcParams = default)
    {
        // Сервер получил запрос — применяем урон
        _stateHolder.health.Value = Mathf.Max(0f, _stateHolder.health.Value - damage);
        Debug.Log($"[ServerRpc] Health now = {_stateHolder.health.Value} for {gameObject.name}");
    }
}