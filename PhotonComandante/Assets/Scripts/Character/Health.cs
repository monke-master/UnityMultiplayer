using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Health : MonoBehaviourPunCallbacks
{
    private StateHolder _stateHolder;
    private PhotonView _photonView;

    private void Awake()
    {
        _stateHolder = GetComponent<StateHolder>();
        _photonView = GetComponent<PhotonView>();
    }

    // Вызывается локально - всегда отправляем RPC владельцу объекта для обработки урона
    public void Damage(float damage)
    {
        if (_photonView == null)
        {
            Debug.LogWarning($"[Health] PhotonView is null on {gameObject.name}. Cannot apply damage.");
            return;
        }
        
        int ownerId = _photonView.OwnerActorNr;
        bool isMine = _photonView.IsMine;
        
        // Если мы владелец объекта, применяем урон напрямую
        // Иначе отправляем RPC всем (включая владельца), но только владелец обработает его
        if (isMine)
        {
            Debug.Log($"[Health] Damage called by owner for {gameObject.name} (Owner: {ownerId}). Applying damage directly.");
            ApplyDamage(damage);
        }
        else
        {
            Debug.Log($"[Health] Damage called on {gameObject.name} (Owner: {ownerId}, IsMine: {isMine}). Sending RPC to all (owner will process).");
            _photonView.RPC(nameof(DamageRpc), RpcTarget.All, damage);
        }
    }

    [PunRPC]
    private void DamageRpc(float damage)
    {
        // Только владелец применяет урон
        if (!_photonView.IsMine)
        {
            return; // Не владелец - игнорируем
        }
        
        int ownerId = _photonView != null ? _photonView.OwnerActorNr : -1;
        Debug.Log($"[Health] DamageRpc received by owner for {gameObject.name} (Owner: {ownerId})");
        
        ApplyDamage(damage);
    }

    private void ApplyDamage(float damage)
    {
        if (_stateHolder == null)
        {
            Debug.LogWarning($"[Health] StateHolder is null on {gameObject.name}");
            return;
        }
        
        float oldHealth = _stateHolder.health;
        // Если damage отрицательный - это лечение, иначе - урон
        _stateHolder.health = _stateHolder.health - damage;
        // Ограничиваем здоровье снизу и сверху
        _stateHolder.health = Mathf.Clamp(_stateHolder.health, 0f, _stateHolder.MAX_HEALTH);
        int ownerId = _photonView != null ? _photonView.OwnerActorNr : -1;
        Debug.Log($"[Health] Applied {(damage < 0 ? "healing" : "damage")}: {Mathf.Abs(damage)}. Health: {oldHealth} -> {_stateHolder.health} for {gameObject.name} (Owner: {ownerId})");
        
        // Проверяем смерть игрока и уведомляем о необходимости проверить победителя
        if (_stateHolder.health <= 0 && oldHealth > 0)
        {
            CheckForGameEnd();
        }
    }
    
    private void CheckForGameEnd()
    {
        Debug.Log($"[Health] Player died! Checking for game end. IsMasterClient: {PhotonNetwork.IsMasterClient}");
        
        // Уведомляем MultiplayerGameController о смерти игрока
        if (MultiplayerGameController.Instance != null)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                // Если мы мастер-клиент, проверяем победителя напрямую
                Debug.Log("[Health] Master client - calling CheckWinner directly");
                MultiplayerGameController.Instance.CheckWinner();
            }
            else
            {
                // Если мы не мастер-клиент, отправляем событие мастеру для проверки
                // Используем RaiseEvent для уведомления мастер-клиента
                Debug.Log("[Health] Not master client - sending RaiseEvent to master");
                bool success = PhotonNetwork.RaiseEvent(
                    (byte)1, // Event code для проверки победителя
                    null,
                    new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
                    SendOptions.SendReliable
                );
                Debug.Log($"[Health] RaiseEvent sent. Success: {success}");
            }
        }
        else
        {
            Debug.LogWarning("[Health] MultiplayerGameController.Instance is null!");
        }
    }
}
