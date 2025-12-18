using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LimitPlayers : MonoBehaviour
{
    [SerializeField] private int maxPlayers = 2;

    private void OnEnable()
    {
        if (NetworkManager.Singleton.ConnectionApprovalCallback == null)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        int currentPlayerCount = NetworkManager.Singleton.ConnectedClientsIds.Count;

        if (currentPlayerCount >= maxPlayers)
        {
            Debug.Log($"Rejecting connection from {request.ClientNetworkId}: lobby full ({currentPlayerCount}/{maxPlayers})");
            response.Approved = false;
            return;
        }

        // Otherwise, approve
        response.Approved = true;
        response.CreatePlayerObject = false; // You handle spawning manually
    }
}