using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerGameController : MonoBehaviour
{

    public static int playerCount = 2;
    
    public List<GameObject> players = new List<GameObject>();
    public List<GameObject> currentPlayers = new List<GameObject>();

    public static int victoryPlayer = 0;
    
    void Start()
    {
        for (int i = 0; i < playerCount; i++)
        {
            currentPlayers.Add(players[i]);
            players[i].SetActive(true);
            SetupCamera(players[i]);
        }
    }
    
    
    void Update()
    {
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
            var health = currentPlayers[i].GetComponentInChildren<StateHolder>().health.Value;
            if (health <= 0) deadPlayers++;
            else winner = i;
        }

        if (deadPlayers != playerCount - 1) return -1;
        return winner;
    }

    private void SetupCamera(GameObject gameObj)
    {
        var camera = gameObj.GetComponentInChildren<Camera>();
        var cameraHeight = 1f;

        if (playerCount > 2) cameraHeight = 0.5f;
        var playerNumber = gameObj.GetComponentInChildren<PlayerInputController>().playerId;

        switch (playerNumber)
        {
            case 1:
                camera.rect = new Rect(0f, 0f, 0.5f, cameraHeight);
                break;
            case 2:
                camera.rect = new Rect(0.5f, 0f, 0.5f, cameraHeight);
                break;
            case 3:
                var width = 1f;
                if (playerCount == 4) width = 0.5f;
                camera.rect = new Rect(0f, 0.5f, width, cameraHeight);
                break;
            case 4:
                camera.rect = new Rect(0.5f, 0.5f, 0.5f, cameraHeight);
                break;
        }
    }
}
