using System.Collections;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// Аптечка, которая восстанавливает здоровье игрока.
/// Синхронизируется через Photon RPC.
/// Мигает перед исчезновением и исчезает по таймеру.
/// </summary>
public class HealthPack : MonoBehaviourPunCallbacks
{
    [Header("Health Settings")]
    [SerializeField] private float healAmount = 50f; // Количество здоровья для восстановления
    
    [Header("Lifetime Settings")]
    [SerializeField] private float lifetime = 20f; // Время жизни аптечки в секундах
    [SerializeField] private float blinkStartTime = 5f; // За сколько секунд до исчезновения начинать мигать
    
    [Header("Blink Settings")]
    [SerializeField] private float blinkInterval = 0.2f; // Интервал мигания в секундах
    
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;
    private bool _isPickedUp = false;
    private bool _isBlinking = false;
    private float _spawnTime;
    private Color _originalColor;
    private HealthPackSpawner _spawner;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
        
        if (_spriteRenderer != null)
        {
            _originalColor = _spriteRenderer.color;
        }
        
        _spawnTime = Time.time;
    }

    private void Start()
    {
        // Находим спавнер для уведомления о разрушении
        _spawner = FindObjectOfType<HealthPackSpawner>();
        
        // Запускаем таймер жизни только на мастер-клиенте
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(LifetimeCoroutine());
        }
    }

    private IEnumerator LifetimeCoroutine()
    {
        // Ждем до начала мигания
        float timeUntilBlink = lifetime - blinkStartTime;
        if (timeUntilBlink > 0)
        {
            yield return new WaitForSeconds(timeUntilBlink);
        }
        
        // Начинаем мигание
        if (!_isPickedUp)
        {
            photonView.RPC(nameof(StartBlinking), RpcTarget.All);
        }
        
        // Ждем оставшееся время до исчезновения
        yield return new WaitForSeconds(blinkStartTime);
        
        // Уничтожаем аптечку, если она еще не была подобрана
        if (!_isPickedUp)
        {
            photonView.RPC(nameof(DestroyHealthPack), RpcTarget.All);
        }
    }

    [PunRPC]
    private void StartBlinking()
    {
        if (_isBlinking || _isPickedUp) return;
        
        _isBlinking = true;
        StartCoroutine(BlinkCoroutine());
    }

    private IEnumerator BlinkCoroutine()
    {
        while (_isBlinking && !_isPickedUp)
        {
            // Мигание: меняем прозрачность
            if (_spriteRenderer != null)
            {
                Color color = _spriteRenderer.color;
                color.a = color.a > 0.5f ? 0.2f : 1f;
                _spriteRenderer.color = color;
            }
            
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    [PunRPC]
    private void DestroyHealthPack()
    {
        if (_isPickedUp) return;
        
        _isPickedUp = true;
        _isBlinking = false;
        
        // Уведомляем спавнер
        if (_spawner != null)
        {
            _spawner.OnHealthPackDestroyed();
        }
        
        // Уничтожаем объект
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Обрабатываем подбор только на мастер-клиенте для предотвращения дублирования
        if (!PhotonNetwork.IsMasterClient || _isPickedUp) return;
        
        if (other.CompareTag("Player"))
        {
            var playerPhotonView = other.GetComponent<PhotonView>();
            if (playerPhotonView == null)
            {
                playerPhotonView = other.GetComponentInParent<PhotonView>();
            }
            if (playerPhotonView == null)
            {
                playerPhotonView = other.GetComponentInChildren<PhotonView>();
            }
            
            if (playerPhotonView != null)
            {
                var health = other.GetComponent<Health>();
                if (health == null)
                {
                    health = other.GetComponentInParent<Health>();
                }
                if (health == null)
                {
                    health = other.GetComponentInChildren<Health>();
                }
                
                if (health != null)
                {
                    // Отправляем RPC всем клиентам для подбора аптечки
                    photonView.RPC(nameof(PickUpHealthPack), RpcTarget.All, playerPhotonView.OwnerActorNr);
                }
            }
        }
    }

    [PunRPC]
    private void PickUpHealthPack(int playerActorNumber)
    {
        if (_isPickedUp) return;
        
        _isPickedUp = true;
        _isBlinking = false;
        
        // Восстанавливаем здоровье только владельцу игрока
        if (PhotonNetwork.LocalPlayer.ActorNumber == playerActorNumber)
        {
            var players = FindObjectsOfType<PhotonView>();
            foreach (var pv in players)
            {
                if (pv.OwnerActorNr == playerActorNumber && pv.CompareTag("Player"))
                {
                    var health = pv.GetComponent<Health>();
                    if (health == null)
                    {
                        health = pv.GetComponentInParent<Health>();
                    }
                    if (health == null)
                    {
                        health = pv.GetComponentInChildren<Health>();
                    }
                    
                    if (health != null)
                    {
                        // Восстанавливаем здоровье (отрицательный урон = лечение)
                        health.Damage(-healAmount);
                        Debug.Log($"[HealthPack] Player {playerActorNumber} picked up health pack, healed {healAmount} HP");
                    }
                    break;
                }
            }
        }
        
        // Уведомляем спавнер
        if (_spawner != null)
        {
            _spawner.OnHealthPackDestroyed();
        }
        
        // Уничтожаем объект
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Останавливаем все корутины
        StopAllCoroutines();
    }
}

