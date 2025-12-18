using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerGameController : NetworkBehaviour
{

    public static MultiplayerGameController Instance;
    
    public GameObject playerPrefab;
    public static int playerCount = 2;
    
    public List<GameObject> currentPlayers = new List<GameObject>();
    
    public List<Transform> playerSpawnPoints = new List<Transform>();

    public static int victoryPlayer = 0;

    public NetworkVariable<ulong> winPlayer = new NetworkVariable<ulong>(228, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    public override void OnNetworkSpawn()
    {
        Instance = this;

        NetworkManager.Singleton.OnClientDisconnectCallback += (clientId) =>
        {
            SceneManager.LoadScene("MenuScene");
            NetworkManager.Singleton.Shutdown();
        };
        
        Debug.Log("OnNetworkSpawn");
        if (!IsServer) return;

        int i = 0;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform spawn = GetSpawnPointForClient(i);
            GameObject player = Instantiate(playerPrefab, spawn.position, spawn.rotation);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            player.GetComponentInChildren<StateHolder>().clientId.Value = clientId;
            currentPlayers.Add(player);
            i++;
        }
    }

    private Transform GetSpawnPointForClient(int index)
    {
        return playerSpawnPoints[index];
    }

    void Update()
    {
        if (!IsServer) return;
        
        var winner = GetDeadPlayers();
        if (winner != -1)
        {
            victoryPlayer = winner + 1;
            LevelController.OnLevelComplete();
        }
    }

    private int GetDeadPlayers()
    {
        int deadPlayers = 0;
        int winner = -1;
        for (int i = 0; i < currentPlayers.Count; i++)
        {
            var stateHolder = currentPlayers[i].GetComponentInChildren<StateHolder>();
            var health = stateHolder.health.Value;
            
            if (health <= 0) deadPlayers++;
            else
            {
                Debug.Log("winner" + stateHolder.clientId.Value);
                winner = i;
                winPlayer.Value = stateHolder.clientId.Value;
            };
        }

        if (deadPlayers != playerCount - 1) return -1;
        return winner;
    }
}
