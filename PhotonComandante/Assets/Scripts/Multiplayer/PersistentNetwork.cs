using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Photon лаунчер: подключение к мастер-серверу, создание/вход в комнату и запуск сцен.
/// </summary>
public class PersistentNetwork : MonoBehaviourPunCallbacks
{
    [Header("Scenes")]
    public string lobbySceneName = "HostScene";
    public string gameSceneName = "Level1";
    public string endSceneName = "VictoryScene";

    [Header("Room")]
    public string defaultRoomName = "Room1";
    public byte maxPlayers = 2;

    private bool _pendingCreateRoom = false;
    private string _pendingRoomName = string.Empty;
    private bool _isExitingToMenu = false; // Флаг для намеренного выхода в меню

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.SendRate = 60;         // сколько раз в секунду Photon отправляет пакеты
        PhotonNetwork.SerializationRate = 30;
        // Не подключаемся автоматически - подключение будет по запросу (Host/Connect)
    }

    #region Connection & Lobby
    public override void OnConnectedToMaster()
    {
        // Переход в лобби после подключения
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        // Если была отложенная попытка создать комнату, создаем её сейчас
        if (_pendingCreateRoom)
        {
            _pendingCreateRoom = false;
            string roomName = string.IsNullOrEmpty(_pendingRoomName) ? defaultRoomName : _pendingRoomName;
            _pendingRoomName = string.Empty;
            
            var options = new RoomOptions { MaxPlayers = maxPlayers };
            PhotonNetwork.CreateRoom(string.IsNullOrWhiteSpace(roomName) ? defaultRoomName : roomName, options);
            return;
        }
        
        // При заходе в лобби можно вернуть игрока в сцену с UI хоста/клиента
        // Но не перекидываем, если игрок на ConnectScene, MenuScene или VictoryScene
        string currentScene = SceneManager.GetActiveScene().name;
        bool isConnectScene = currentScene == "ConnectScene" || currentScene == "Scenes/ConnectScene";
        bool isMenuScene = currentScene == "MenuScene" || currentScene == "Scenes/MenuScene";
        bool isVictoryScene = currentScene == "VictoryScene" || currentScene == "Scenes/VictoryScene";
        
        // Не перекидываем на HostScene, если игрок на ConnectScene, MenuScene или VictoryScene
        // И не перекидываем, если игрок уже в комнате (возможно, остался после игры)
        if (!string.IsNullOrEmpty(lobbySceneName) && currentScene != lobbySceneName && !isConnectScene && !isMenuScene && !isVictoryScene && !PhotonNetwork.InRoom)
        {
            SceneManager.LoadScene(lobbySceneName);
        }
    }
    #endregion

    #region Room Management
    public void CreateRoom(string roomName)
    {
        // Убеждаемся, что подключены перед созданием комнаты
        if (!PhotonNetwork.IsConnected)
        {
            _pendingCreateRoom = true;
            _pendingRoomName = string.IsNullOrWhiteSpace(roomName) ? defaultRoomName : roomName;
            PhotonNetwork.ConnectUsingSettings();
            // Создание комнаты произойдет после подключения в OnJoinedLobby
            return;
        }
        
        // Если не в лобби, входим в лобби
        if (!PhotonNetwork.InLobby)
        {
            _pendingCreateRoom = true;
            _pendingRoomName = string.IsNullOrWhiteSpace(roomName) ? defaultRoomName : roomName;
            PhotonNetwork.JoinLobby();
            // Создание комнаты произойдет после входа в лобби
            return;
        }
        
        var options = new RoomOptions { MaxPlayers = maxPlayers };
        PhotonNetwork.CreateRoom(string.IsNullOrWhiteSpace(roomName) ? defaultRoomName : roomName, options);
    }

    public void JoinRoom(string roomName)
    {
        // Убеждаемся, что подключены перед присоединением к комнате
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            // Присоединение произойдет после подключения - нужно будет вызвать JoinRoom снова
            return;
        }
        
        // Если не в лобби, входим в лобби
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
            // Присоединение произойдет после входа в лобби - нужно будет вызвать JoinRoom снова
            return;
        }
        
        PhotonNetwork.JoinRoom(string.IsNullOrWhiteSpace(roomName) ? defaultRoomName : roomName);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // Если нет комнат — создаём новую
        CreateRoom(defaultRoomName);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"Create room failed: {message}. Trying default room.");
        if (!PhotonNetwork.InRoom) CreateRoom(defaultRoomName + Random.Range(0, 9999));
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"Join room failed: {message}. Trying random.");
    }

    public override void OnJoinedRoom()
    {
        // После входа в комнату переходим на HostScene только когда игрок создал комнату (из MenuScene)
        // НЕ перекидываем, если игрок на ConnectScene (он сам решит, когда присоединяться)
        string currentScene = SceneManager.GetActiveScene().name;
        bool isMenuScene = currentScene == "MenuScene" || currentScene == "Scenes/MenuScene";
        bool isConnectScene = currentScene == "ConnectScene" || currentScene == "Scenes/ConnectScene";
        bool isAlreadyOnHostScene = currentScene == lobbySceneName || currentScene == "Scenes/" + lobbySceneName;
        bool isVictoryScene = currentScene == "VictoryScene" || currentScene == "Scenes/VictoryScene";
        
        // Перекидываем на HostScene только если игрок в меню и создал комнату
        // НЕ перекидываем, если игрок на ConnectScene, VictoryScene или уже на HostScene
        if (!string.IsNullOrEmpty(lobbySceneName) && !isAlreadyOnHostScene && isMenuScene && !isConnectScene && !isVictoryScene)
        {
            SceneManager.LoadScene(lobbySceneName);
        }
    }
    #endregion

    #region Game Flow
    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        PhotonNetwork.LoadLevel(gameSceneName);
    }

    public void EndGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        PhotonNetwork.LoadLevel(endSceneName);
    }

    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom) PhotonNetwork.LeaveRoom();
    }
    
    public void LeaveRoomAndGoToMenu()
    {
        // Отключаем автоматическую синхронизацию сцены, чтобы избежать ошибок при загрузке меню
        PhotonNetwork.AutomaticallySyncScene = false;
        
        // Выходим из комнаты, если мы в ней (но не ждем колбэка)
        if (PhotonNetwork.InRoom) 
        {
            PhotonNetwork.LeaveRoom();
        }
        
        // Отключаемся от Photon и сразу загружаем меню
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        
        SceneManager.LoadScene("MenuScene");
    }
    
    public override void OnDisconnected(DisconnectCause cause)
    {
        // Восстанавливаем автоматическую синхронизацию сцены при переподключении
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    #endregion
}