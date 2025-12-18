using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentNetwork : MonoBehaviour
{
    public string hostSceneName = "HostScene";
    public string gameSceneName = "Level1";
    public string endSceneName = "VictoryScene";

    private void Awake()
    {
        // DontDestroyOnLoad(gameObject);

        // Опционально: если Singleton уже существует — удалить дубликат
        if (NetworkManager.Singleton == null)
        {
            Debug.Log("NetworkManager singleton not found yet.");
        }
    }

    // Вызывается из MenuScene -> Host
    public void StartHostAndGotoHostScene()
    {
        StartCoroutine(StartHostDelayed());
    }
    
    private IEnumerator StartHostDelayed()
    {
        yield return new WaitUntil(() => !NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer);

        // Reset transport
        UnityTransport transport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        transport.Shutdown();
        transport.SetConnectionData("0.0.0.0", 7777); // valid host endpoint

        Debug.Log("Starting host...");
        NetworkManager.Singleton.StartHost();

        // Wait until host is active
        yield return new WaitUntil(() => NetworkManager.Singleton.IsServer);

        Debug.Log("Loading host scene...");
        NetworkManager.Singleton.SceneManager.LoadScene(hostSceneName, LoadSceneMode.Single);
    }

    // Вызывается из MenuScene -> Client (передать IP/порт)
    public void StartClientWithIP(string ip, ushort port = 7777)
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, port);
        NetworkManager.Singleton.StartClient();
    }

    // Хост вызывает, чтобы начать игру
    public void HostStartGame()
    {
        if (!NetworkManager.Singleton.IsHost) return;
        NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }

    // Хост вызывает, чтобы закончить и показать результаты
    public void HostEndGame()
    {
        if (!NetworkManager.Singleton.IsHost) return;
        NetworkManager.Singleton.SceneManager.LoadScene(endSceneName, LoadSceneMode.Single);
    }

    // В любой момент можно корректно закрыть сессию
    public void ShutdownNetwork()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.Shutdown();
    }
}