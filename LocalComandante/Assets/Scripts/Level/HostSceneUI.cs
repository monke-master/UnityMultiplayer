using System.Linq;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HostSceneUI : MonoBehaviour
{
    public Text ipText;
    public Text playersCountText;
    public Button startGameButton;

    private void Start()
    {
        UpdatePlayersCount();

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        // Только хост может нажать Start
        startGameButton.interactable = NetworkManager.Singleton.IsHost;
        startGameButton.onClick.AddListener(OnStartGameClicked);

        ipText.GameObject().SetActive(NetworkManager.Singleton.IsHost);
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId) => UpdatePlayersCount();
    private void OnClientDisconnected(ulong clientId) => UpdatePlayersCount();

    private void UpdatePlayersCount()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            playersCountText.text = "Waiting for host starts the game";
            return;
        };
        
        if (NetworkManager.Singleton == null) return;
        int count = NetworkManager.Singleton.ConnectedClients.Count;
        playersCountText.text = $"Players: {count}";
    }
    
    private void OnStartGameClicked()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogWarning("Only host can start the game.");
            return;
        }

        Debug.Log("Starting game session...");

        // Хост инициирует загрузку сцены для всех клиентов
        NetworkManager.Singleton.SceneManager.LoadScene("Level1", LoadSceneMode.Single);
    }

    public void ExitGame()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("MenuScene");
    }
}
