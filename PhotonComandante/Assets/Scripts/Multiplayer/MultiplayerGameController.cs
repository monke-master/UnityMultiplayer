using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerGameController : MonoBehaviourPunCallbacks
{
    public static MultiplayerGameController Instance;
    
    public GameObject playerPrefab;
    public static int playerCount = 2;
    
    public List<GameObject> currentPlayers = new List<GameObject>();
    public List<Transform> playerSpawnPoints = new List<Transform>();

    public static int victoryPlayer = 0;
    public static long winPlayerClientId = -1; // Статическая переменная для синхронизации

    private bool _spawnedLocal = false;

    private void Awake()
    {
        Instance = this;
    }
    
    private void OnEnable()
    {
        // Подписываемся на событие загрузки сцены
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Подписываемся на события Photon для проверки победителя
        SubscribeToPhotonEvents();
    }
    
    private void OnDisable()
    {
        // Отписываемся от событий
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeFromPhotonEvents();
    }
    
    private void SubscribeToPhotonEvents()
    {
        if (PhotonNetwork.NetworkingClient != null)
        {
            PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
            Debug.Log("[MultiplayerGameController] Subscribed to Photon events");
        }
        else
        {
            Debug.LogWarning("[MultiplayerGameController] PhotonNetwork.NetworkingClient is null, cannot subscribe to events");
        }
    }
    
    private void UnsubscribeFromPhotonEvents()
    {
        if (PhotonNetwork.NetworkingClient != null)
        {
            PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
        }
    }
    
    public override void OnConnectedToMaster()
    {
        // Подписываемся на события когда подключились к мастеру
        SubscribeToPhotonEvents();
    }
    
    private void OnEvent(EventData photonEvent)
    {
        // Обрабатываем событие проверки победителя
        Debug.Log($"[MultiplayerGameController] Event received. Code: {photonEvent.Code}, IsMasterClient: {PhotonNetwork.IsMasterClient}");
        if (photonEvent.Code == 1 && PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[MultiplayerGameController] Received check winner event from client - calling CheckWinner");
            CheckWinner();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Когда сцена загружена, сбрасываем флаг спавна и спауним игрока
        _spawnedLocal = false;
        currentPlayers.Clear();
        
        // Небольшая задержка, чтобы убедиться что все клиенты загрузили сцену
        StartCoroutine(DelayedSpawn());
    }
    
    private System.Collections.IEnumerator DelayedSpawn()
    {
        yield return new WaitForSeconds(0.5f);
        SpawnLocalPlayerIfNeeded();
        yield return new WaitForSeconds(0.5f);
        UpdatePlayersList();
    }

    public override void OnJoinedRoom()
    {
        SpawnLocalPlayerIfNeeded();
        
        // Загружаем ID победителя из Custom Properties если он есть
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("WinnerClientId"))
        {
            winPlayerClientId = (long)PhotonNetwork.CurrentRoom.CustomProperties["WinnerClientId"];
            Debug.Log($"[MultiplayerGameController] Loaded winner from room properties: {winPlayerClientId}");
        }
    }
    
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        // Обновляем ID победителя при изменении свойств комнаты
        if (propertiesThatChanged.ContainsKey("WinnerClientId"))
        {
            winPlayerClientId = (long)propertiesThatChanged["WinnerClientId"];
            Debug.Log($"[MultiplayerGameController] Winner updated from room properties: {winPlayerClientId}");
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player entered room: ActorNumber {newPlayer.ActorNumber}, IsLocal: {newPlayer.IsLocal}");
        // Когда новый игрок входит в комнату, убеждаемся что мы заспаунили своего игрока
        SpawnLocalPlayerIfNeeded();
        
        // Обновляем список всех игроков в комнате
        UpdatePlayersList();
    }
    
    private void UpdatePlayersList()
    {
        // Находим всех игроков в сцене через PhotonView
        PhotonView[] allPhotonViews = FindObjectsOfType<PhotonView>();
        currentPlayers.Clear();
        
        foreach (var pv in allPhotonViews)
        {
            if (pv.CompareTag("Player"))
            {
                if (!currentPlayers.Contains(pv.gameObject))
                {
                    currentPlayers.Add(pv.gameObject);
                    Debug.Log($"Found player in scene: ActorNumber {pv.OwnerActorNr}, IsMine: {pv.IsMine}");
                }
            }
        }
        
        Debug.Log($"Total players in list: {currentPlayers.Count}, Players in room: {PhotonNetwork.CurrentRoom?.PlayerCount ?? 0}");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Удаляем игрока из списка, если он вышел
        for (int i = currentPlayers.Count - 1; i >= 0; i--)
        {
            if (currentPlayers[i] == null)
            {
                currentPlayers.RemoveAt(i);
                continue;
            }
            var photonView = currentPlayers[i].GetComponent<PhotonView>();
            if (photonView != null && photonView.OwnerActorNr == otherPlayer.ActorNumber)
            {
                if (photonView.IsMine)
                {
                    PhotonNetwork.Destroy(currentPlayers[i]);
                }
                currentPlayers.RemoveAt(i);
            }
        }

        // Вернуться в меню, если мастер вышел
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }

        CheckWinner();
    }

    private void SpawnLocalPlayerIfNeeded()
    {
        if (_spawnedLocal) return;
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab is not assigned in MultiplayerGameController.");
            return;
        }
        if (!PhotonNetwork.InRoom) return;
        if (playerSpawnPoints == null || playerSpawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points assigned in MultiplayerGameController.");
            return;
        }

        playerCount = PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.MaxPlayers : playerCount;
        int index = Mathf.Clamp(PhotonNetwork.LocalPlayer.ActorNumber - 1, 0, playerSpawnPoints.Count - 1);
        Transform spawn = GetSpawnPointForClient(index);
        var player = PhotonNetwork.Instantiate(playerPrefab.name, spawn.position, spawn.rotation);
        
        if (player != null)
        {
            currentPlayers.Add(player);
            _spawnedLocal = true;
            Debug.Log($"Spawned local player. ActorNumber: {PhotonNetwork.LocalPlayer.ActorNumber}, IsMasterClient: {PhotonNetwork.IsMasterClient}");
        }
    }

    private Transform GetSpawnPointForClient(int index)
    {
        return playerSpawnPoints[index % playerSpawnPoints.Count];
    }

    private float _lastCheckTime = 0f;
    private const float CHECK_INTERVAL = 0.2f; // Проверяем каждые 0.2 секунды
    
    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        // Периодическая проверка победителя
        if (Time.time - _lastCheckTime >= CHECK_INTERVAL)
        {
            _lastCheckTime = Time.time;
            CheckWinner();
        }
    }

    public void CheckWinner()
    {
        Debug.Log("[MultiplayerGameController] CheckWinner called");
        
        // Обновляем список игроков перед проверкой
        UpdatePlayersList();
        
        // Нужно минимум 2 игрока для завершения игры
        if (currentPlayers.Count < 2)
        {
            Debug.Log($"[MultiplayerGameController] Not enough players: {currentPlayers.Count}");
            return;
        }
        
        int deadPlayers = 0;
        int winnerIndex = -1;
        int alivePlayers = 0;
        StateHolder winnerStateHolder = null;

        Debug.Log($"[MultiplayerGameController] Checking {currentPlayers.Count} players...");
        for (int i = 0; i < currentPlayers.Count; i++)
        {
            if (currentPlayers[i] == null)
            {
                Debug.Log($"[MultiplayerGameController] Player {i} is null");
                continue;
            }
            
            // Пробуем найти StateHolder разными способами
            var stateHolder = currentPlayers[i].GetComponent<StateHolder>();
            if (stateHolder == null)
            {
                stateHolder = currentPlayers[i].GetComponentInChildren<StateHolder>();
            }
            if (stateHolder == null)
            {
                stateHolder = currentPlayers[i].GetComponentInParent<StateHolder>();
            }
            
            if (stateHolder == null)
            {
                Debug.Log($"[MultiplayerGameController] Player {i} ({currentPlayers[i].name}) has no StateHolder");
                continue;
            }

            Debug.Log($"[MultiplayerGameController] Player {i} ({currentPlayers[i].name}): Health = {stateHolder.health}, ClientId = {stateHolder.clientId}, OwnerActorNr = {currentPlayers[i].GetComponent<PhotonView>()?.OwnerActorNr ?? -1}");
            
            if (stateHolder.health <= 0)
            {
                deadPlayers++;
                Debug.Log($"[MultiplayerGameController] Player {i} is DEAD (health: {stateHolder.health})");
            }
            else
            {
                alivePlayers++;
                winnerIndex = i;
                winnerStateHolder = stateHolder;
                Debug.Log($"[MultiplayerGameController] Player {i} is ALIVE (health: {stateHolder.health}, potential winner)");
            }
        }

        Debug.Log($"[MultiplayerGameController] Summary: Alive={alivePlayers}, Dead={deadPlayers}, WinnerIndex={winnerIndex}");

        // Если остался только один живой игрок, завершаем игру
        if (alivePlayers == 1 && deadPlayers > 0 && winnerIndex != -1 && winnerStateHolder != null)
        {
            // Сохраняем ID победителя в статическую переменную и в Custom Properties комнаты
            winPlayerClientId = (long)winnerStateHolder.clientId;
            
            // Сохраняем в Custom Properties для синхронизации между клиентами
            var props = new ExitGames.Client.Photon.Hashtable();
            props["WinnerClientId"] = winPlayerClientId;
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            
            Debug.Log($"[MultiplayerGameController] GAME ENDED! Winner: Player {winnerIndex + 1} (ClientId: {winPlayerClientId})");
            victoryPlayer = winnerIndex + 1;
            LevelController.OnLevelComplete();
        }
        else
        {
            Debug.Log($"[MultiplayerGameController] Game continues. Condition not met: alivePlayers={alivePlayers}, deadPlayers={deadPlayers}, winnerIndex={winnerIndex}");
        }
    }
}