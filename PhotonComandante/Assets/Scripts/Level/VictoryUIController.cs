using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryUIController : MonoBehaviourPunCallbacks
{
    private bool _isDisconnecting = false;

    private void Awake()
    {
        var scoreText = transform.Find("ScoreText").GetComponent<Text>();
        
        // Используем статическую переменную или Custom Properties комнаты
        long winnerId = MultiplayerGameController.winPlayerClientId;
        
        // Если статическая переменная не установлена, пытаемся получить из Custom Properties
        if (winnerId == -1 && PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("WinnerClientId"))
        {
            winnerId = (long)PhotonNetwork.CurrentRoom.CustomProperties["WinnerClientId"];
            MultiplayerGameController.winPlayerClientId = winnerId;
        }
        
        long localPlayerId = PhotonNetwork.LocalPlayer != null ? PhotonNetwork.LocalPlayer.ActorNumber : -1;
        bool isWinner = (winnerId == localPlayerId);
        
        Debug.Log($"[VictoryUIController] Winner ID: {winnerId}, Local Player ID: {localPlayerId}, IsWinner: {isWinner}");
        
        var text = isWinner ? "You are winner" : "You've lost";
        scoreText.text = text;
    }

    public void ToMenu()
    {
        if (_isDisconnecting) return;
        
        _isDisconnecting = true;
        
        // Отключаем автоматическую синхронизацию сцены перед отключением
        PhotonNetwork.AutomaticallySyncScene = false;
        
        // Выходим из комнаты перед отключением
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            // Если не в комнате, просто отключаемся
            StartCoroutine(DisconnectAndLoadMenu());
        }
    }
    
    public override void OnLeftRoom()
    {
        // После выхода из комнаты отключаемся и загружаем меню
        StartCoroutine(DisconnectAndLoadMenu());
    }
    
    private IEnumerator DisconnectAndLoadMenu()
    {
        // Отключаемся от Photon
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            
            // Ждем отключения (максимум 2 секунды)
            float timeout = 2f;
            float elapsed = 0f;
            while (PhotonNetwork.IsConnected && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        
        // Загружаем меню после отключения
        LevelController.ToMenu();
    }
    
    public override void OnDisconnected(DisconnectCause cause)
    {
        // Если отключились, загружаем меню
        if (_isDisconnecting)
        {
            LevelController.ToMenu();
        }
    }
}
