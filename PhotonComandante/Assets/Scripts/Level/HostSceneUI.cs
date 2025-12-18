using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HostSceneUI : MonoBehaviourPunCallbacks
{
    public Text roomNameText;
    public Text playersCountText;
    public Button startGameButton;

    private PersistentNetwork _lobby;

    private void Awake()
    {
        _lobby = FindObjectOfType<PersistentNetwork>();
    }

    private void Start()
    {
        EnsureRoomExists();
        UpdatePlayersCount();

        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(OnStartGameClicked);
            
            // Скрываем кнопку Start для клиента, показываем только для хоста
            bool isMaster = PhotonNetwork.IsMasterClient;
            startGameButton.interactable = isMaster;
            startGameButton.gameObject.SetActive(isMaster);
        }

        if (roomNameText != null)
        {
            roomNameText.text = PhotonNetwork.InRoom ? $"Room: {PhotonNetwork.CurrentRoom.Name}" : "No Room";
        }
    }

    public override void OnJoinedRoom()
    {
        if (roomNameText != null)
        {
            roomNameText.text = $"Room: {PhotonNetwork.CurrentRoom.Name}";
        }
        
        // Обновляем видимость и доступность кнопки Start
        if (startGameButton != null)
        {
            bool isMaster = PhotonNetwork.IsMasterClient;
            startGameButton.interactable = isMaster;
            startGameButton.gameObject.SetActive(isMaster);
        }
        
        UpdatePlayersCount();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) => UpdatePlayersCount();
    public override void OnPlayerLeftRoom(Player otherPlayer) => UpdatePlayersCount();

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // Обновляем видимость и доступность кнопки Start при смене хоста
        if (startGameButton != null)
        {
            bool isMaster = PhotonNetwork.IsMasterClient;
            startGameButton.interactable = isMaster;
            startGameButton.gameObject.SetActive(isMaster);
        }
    }

    private void EnsureRoomExists()
    {
        if (_lobby == null) _lobby = FindObjectOfType<PersistentNetwork>();
        
        // Не создаем комнату автоматически, если мы в меню или намеренно выходим
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene == "MenuScene" || currentScene == "Scenes/MenuScene")
        {
            return;
        }
        
        if (PhotonNetwork.InRoom || _lobby == null || !PhotonNetwork.IsConnected) return;

        var name = string.IsNullOrWhiteSpace(_lobby.defaultRoomName) ? "Room1" : _lobby.defaultRoomName;
        _lobby.CreateRoom(name);
        if (roomNameText != null) roomNameText.text = $"Room: {name} (creating)";
    }

    private void UpdatePlayersCount()
    {
        if (!PhotonNetwork.InRoom)
        {
            playersCountText.text = "Not in room";
            return;
        }

        int count = PhotonNetwork.CurrentRoom.PlayerCount;
        int max = PhotonNetwork.CurrentRoom.MaxPlayers;
        playersCountText.text = $"Players: {count}/{max}";
    }
    
    private void OnStartGameClicked()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("Only host can start the game.");
            return;
        }

        Debug.Log("Starting game session...");
        _lobby?.StartGame();
    }

    public void ExitGame()
    {
        // Используем специальный метод для выхода в меню
        if (_lobby != null)
        {
            _lobby.LeaveRoomAndGoToMenu();
        }
        else
        {
            // Если лобби не найдено, просто выходим из комнаты и загружаем меню
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
            }
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Disconnect();
            }
            SceneManager.LoadScene("MenuScene");
        }
    }
}
