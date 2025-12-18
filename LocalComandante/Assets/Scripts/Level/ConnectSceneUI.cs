using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConnectSceneUI : MonoBehaviour
{
    public InputField ipInput;
    public InputField portInput;
    public Button connectButton;
    public Text statusText;

    public float connectTimeoutSeconds = 8f;

    private Coroutine connectTimeoutCoroutine;

    private void Start()
    {
        connectButton.onClick.AddListener(OnConnectClicked);
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnConnectClicked()
    {
        string ip = string.IsNullOrWhiteSpace(ipInput.text) ? "127.0.0.1" : ipInput.text;
        ushort port = 7777;
        if (!string.IsNullOrWhiteSpace(portInput.text)) ushort.TryParse(portInput.text, out port);

        statusText.text = "Connecting...";
        connectButton.interactable = false;

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, port);
        NetworkManager.Singleton.StartClient();

        if (connectTimeoutCoroutine != null) StopCoroutine(connectTimeoutCoroutine);
        connectTimeoutCoroutine = StartCoroutine(ConnectTimeout());
    }

    private IEnumerator ConnectTimeout()
    {
        float t = 0f;
        while (t < connectTimeoutSeconds)
        {
            t += Time.deltaTime;
            yield return null;
            // Если подключились, корутина будет остановлена в OnClientConnected
        }

        // Если по таймауту всё ещё не подключились
        statusText.text = "Error: connection time out";
        connectButton.interactable = true;
        try
        {
            NetworkManager.Singleton.Shutdown();
        }
        catch { }
        
    }

    private void OnClientConnected(ulong clientId)
    {
        // Клиент видит свой clientId; когда клиент подключился (clientId == LocalClientId)
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            if (connectTimeoutCoroutine != null) StopCoroutine(connectTimeoutCoroutine);
            statusText.text = "Connected. Waiting for host starts the game...";
            // Клиент не должен загружать сцену — хост сделает это
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        // Если локальный клиент отключился — показать ошибку
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            if (connectTimeoutCoroutine != null) StopCoroutine(connectTimeoutCoroutine);
            statusText.text = "Connection error";
            connectButton.interactable = true;
        }
        else if (clientId == NetworkManager.ServerClientId)
        {
            SceneManager.LoadScene("MenuScene");
            NetworkManager.Singleton.Shutdown();
        }
    }
    
    public void ExitGame()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("MenuScene");
    }
}
