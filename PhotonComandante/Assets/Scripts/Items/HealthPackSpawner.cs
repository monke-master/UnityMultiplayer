using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

/// <summary>
/// Спавнер аптечек. Работает только на мастер-клиенте.
/// Спавнит аптечку раз в 30 секунд в случайном месте.
/// </summary>
public class HealthPackSpawner : MonoBehaviourPunCallbacks
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject healthPackPrefab;
    [SerializeField] private float spawnInterval = 30f; // Интервал спавна в секундах
    [SerializeField] private Transform[] spawnPoints; // Массив точек спавна (можно оставить пустым для случайного спавна)
    [SerializeField] private Vector2 spawnAreaMin = new Vector2(-10f, -5f); // Минимальные координаты области спавна
    [SerializeField] private Vector2 spawnAreaMax = new Vector2(10f, 5f); // Максимальные координаты области спавна
    
    private GameObject _currentHealthPack;
    private float _lastSpawnTime = 0f;
    private bool _isSpawning = false;

    private void Start()
    {
        // Начинаем спавн только на мастер-клиенте
        if (PhotonNetwork.IsMasterClient)
        {
            _lastSpawnTime = Time.time;
            StartCoroutine(SpawnLoop());
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
        {
            // Ждем интервал спавна
            yield return new WaitForSeconds(spawnInterval);
            
            // Проверяем, что текущая аптечка не существует или уже исчезла
            if (_currentHealthPack == null)
            {
                SpawnHealthPack();
            }
        }
    }

    private void SpawnHealthPack()
    {
        if (_isSpawning) return;
        _isSpawning = true;

        Vector3 spawnPosition = GetRandomSpawnPosition();
        
        // Спавним аптечку через PhotonNetwork для синхронизации
        _currentHealthPack = PhotonNetwork.Instantiate(
            healthPackPrefab.name,
            spawnPosition,
            Quaternion.identity
        );

        Debug.Log($"[HealthPackSpawner] Spawned health pack at {spawnPosition}");
        _isSpawning = false;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        // Если есть точки спавна, используем их
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            return randomPoint.position;
        }
        
        // Иначе используем случайную позицию в заданной области
        float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float y = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
        return new Vector3(x, y, 0f);
    }

    // Метод для уведомления спавнера о том, что аптечка была подобрана или исчезла
    public void OnHealthPackDestroyed()
    {
        _currentHealthPack = null;
    }
}


