using Photon.Pun;
using UnityEngine;

public class Bullet : MonoBehaviourPunCallbacks
{
    [SerializeField] private string targetTag;
    [SerializeField] private float damage;
    [SerializeField] private float ignoreCollisionTime = 0.1f; // Время игнорирования коллизий со стрелком
    
    private bool _hasHit = false; // Предотвращаем множественные попадания
    private PhotonView _photonView;
    private int _shooterActorNumber = -1; // ActorNumber игрока, который выстрелил
    private float _spawnTime;
    private Collider2D _bulletCollider;
    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
        _bulletCollider = GetComponent<Collider2D>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _spawnTime = Time.time;
        
        // Устанавливаем стрелка сразу по владельцу пули (будет обновлено через RPC если нужно)
        if (_photonView != null)
        {
            _shooterActorNumber = _photonView.OwnerActorNr;
        }
        
        // Отключаем гравитацию для пули
        if (_rigidbody != null)
        {
            _rigidbody.gravityScale = 0f;
            _rigidbody.drag = 0f;
        }
    }
    
    private void Start()
    {
        // Игнорируем коллизии со стрелком на короткое время
        IgnoreShooterCollisions();
    }
    
    private void IgnoreShooterCollisions()
    {
        // Используем _shooterActorNumber если установлен, иначе используем владельца пули
        int shooterId = _shooterActorNumber != -1 ? _shooterActorNumber : (_photonView != null ? _photonView.OwnerActorNr : -1);
        
        if (shooterId == -1) return;
        
        // Находим всех игроков в сцене
        PhotonView[] allPhotonViews = FindObjectsOfType<PhotonView>();
        
        foreach (var pv in allPhotonViews)
        {
            if (pv.OwnerActorNr == shooterId && pv.gameObject.CompareTag(targetTag))
            {
                // Игнорируем коллизии между пулей и стрелком
                Collider2D shooterCollider = pv.GetComponent<Collider2D>();
                if (shooterCollider == null)
                {
                    shooterCollider = pv.GetComponentInChildren<Collider2D>();
                }
                
                if (_bulletCollider != null && shooterCollider != null)
                {
                    Physics2D.IgnoreCollision(_bulletCollider, shooterCollider, true);
                    Debug.Log($"[Bullet] Ignoring collision with shooter (ActorNumber: {shooterId})");
                }
            }
        }
    }
    
    // Вызывается через RPC для установки стрелка
    [PunRPC]
    public void SetShooter(int shooterActorNumber)
    {
        _shooterActorNumber = shooterActorNumber;
        IgnoreShooterCollisions();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        HandleHit(other.gameObject);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other.gameObject);
    }
    
    private void HandleHit(GameObject hitObject)
    {
        // Предотвращаем множественные попадания
        if (_hasHit) return;
        
        // Обрабатываем попадание только на клиенте, который создал пулю
        // Это предотвращает двойную обработку
        if (_photonView != null && !_photonView.IsMine)
        {
            return;
        }
        
        // Игнорируем попадание в стрелка в течение короткого времени после спавна
        var hitPhotonView = hitObject.GetComponent<PhotonView>();
        if (hitPhotonView == null)
        {
            hitPhotonView = hitObject.GetComponentInParent<PhotonView>();
        }
        
        // Проверяем попадание в стрелка по владельцу пули или по установленному _shooterActorNumber
        int shooterId = _shooterActorNumber != -1 ? _shooterActorNumber : (_photonView != null ? _photonView.OwnerActorNr : -1);
        
        if (hitPhotonView != null && hitPhotonView.OwnerActorNr == shooterId)
        {
            // Проверяем, прошло ли достаточно времени
            if (Time.time - _spawnTime < ignoreCollisionTime)
            {
                Debug.Log($"[Bullet] Ignoring hit on shooter (ActorNumber: {shooterId}) - too soon after spawn");
                return;
            }
        }
        
        Debug.Log($"[Bullet] Hit detected by owner. Object: {hitObject.name}, Tag: {hitObject.tag}, TargetTag: {targetTag}");
        
        // Проверяем тег цели (может быть на дочернем объекте)
        GameObject targetObject = hitObject;
        if (!hitObject.CompareTag(targetTag))
        {
            // Ищем родительский объект с правильным тегом
            Transform parent = hitObject.transform.parent;
            while (parent != null)
            {
                if (parent.CompareTag(targetTag))
                {
                    targetObject = parent.gameObject;
                    Debug.Log($"[Bullet] Found target tag on parent: {targetObject.name}");
                    break;
                }
                parent = parent.parent;
            }
            
            if (!targetObject.CompareTag(targetTag))
            {
                Debug.Log($"[Bullet] Tag mismatch. Hit: {hitObject.tag}, Expected: {targetTag}");
                return;
            }
        }
        
        _hasHit = true;
        
        // Ищем Health компонент (может быть на дочернем объекте или родителе)
        var health = targetObject.GetComponent<Health>();
        if (health == null)
        {
            health = targetObject.GetComponentInParent<Health>();
        }
        if (health == null)
        {
            health = targetObject.GetComponentInChildren<Health>();
        }
        
        if (health != null)
        {
            var targetPhotonView = targetObject.GetComponent<PhotonView>();
            if (targetPhotonView == null)
            {
                targetPhotonView = targetObject.GetComponentInParent<PhotonView>();
            }
            
            int targetOwnerId = targetPhotonView != null ? targetPhotonView.OwnerActorNr : -1;
            int bulletOwnerId = _photonView != null ? _photonView.OwnerActorNr : -1;
            
            Debug.Log($"[Bullet] Applying damage: {damage} to {targetObject.name} (Target Owner: {targetOwnerId}, Bullet Owner: {bulletOwnerId}, IsMaster: {PhotonNetwork.IsMasterClient})");
            health.Damage(damage);
        }
        else
        {
            Debug.LogWarning($"[Bullet] No Health component found on {targetObject.name} or its parents/children");
        }
        
        // Уничтожаем пулю
        // Если пуля создана через PhotonNetwork.Instantiate, используем PhotonNetwork.Destroy
        if (_photonView != null && _photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            // Fallback: уничтожаем локально
            Destroy(gameObject);
        }
    }
}
