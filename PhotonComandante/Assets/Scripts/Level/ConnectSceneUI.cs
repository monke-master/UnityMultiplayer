using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WebSocketSharp;

public class ConnectSceneUI : MonoBehaviourPunCallbacks
{
    public InputField roomNameInput;
    public Button connectButton;
    public Text statusText;

    private PersistentNetwork _lobby;

    private void Awake()
    {
        _lobby = FindObjectOfType<PersistentNetwork>();
        connectButton.enabled = false;
    }

    private void Start()
    {
        connectButton.onClick.AddListener(OnConnectClicked);
        var needToLeftRoom = false;
        
        // Убеждаемся, что мы не в комнате перед подключением
        if (PhotonNetwork.InRoom)
        {
            needToLeftRoom = true;
            PhotonNetwork.LeaveRoom();
        }
        
        // Автоматически подключаемся к Photon при загрузке ConnectScene, если еще не подключены
        if (!PhotonNetwork.IsConnected && !needToLeftRoom)
        {
            if (_lobby == null) _lobby = FindObjectOfType<PersistentNetwork>();
            PhotonNetwork.ConnectUsingSettings();
            if (statusText != null)
            {
                statusText.text = "Connecting to Photon...";
            }
        }
    }

    public override void OnLeftRoom()
    {
        if (_lobby == null) _lobby = FindObjectOfType<PersistentNetwork>();
        PhotonNetwork.ConnectUsingSettings();
        if (statusText != null)
        {
            statusText.text = "Connecting to Photon...";
        }
    }

    private void OnConnectClicked()
    {
        connectButton.interactable = false;
        statusText.text = "Connecting...";

        if (!PhotonNetwork.IsConnected)
        {
            statusText.text = "Not connected yet. Please wait...";
            connectButton.interactable = true;
            return;
        }

        string roomName = roomNameInput == null ? string.Empty : roomNameInput.text;
        if (_lobby == null) _lobby = FindObjectOfType<PersistentNetwork>();
        _lobby?.JoinRoom(roomName);
    }
    
    public override void OnConnectedToMaster()
    {
        connectButton.enabled = true;
        if (statusText != null)
        {
            statusText.text = "Connected. Enter room name and click Connect.";
        }
    }

    public override void OnJoinedRoom()
    {
        statusText.text = "Joined. Waiting for host to start.";

        if (roomNameInput.text.IsNullOrEmpty())
        {
            PhotonNetwork.LeaveRoom();
            return;
        }
        
        // После присоединения к комнате переходим на HostScene
        // Используем PersistentNetwork для перехода
        if (_lobby == null) _lobby = FindObjectOfType<PersistentNetwork>();
        if (_lobby != null && !string.IsNullOrEmpty(_lobby.lobbySceneName))
        {
            SceneManager.LoadScene(_lobby.lobbySceneName);
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        statusText.text = $"Join failed: {message}";
        connectButton.interactable = true;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        statusText.text = $"Disconnected: {cause}";
        connectButton.interactable = true;
    }

    public void ExitGame()
    {
        // Отключаемся от Photon перед возвратом в меню
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        SceneManager.LoadScene("MenuScene");
    }
}
