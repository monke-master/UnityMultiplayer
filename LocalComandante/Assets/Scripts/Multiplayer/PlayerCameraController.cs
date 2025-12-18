using Unity.Netcode;
using UnityEngine;

public class PlayerCameraController : NetworkBehaviour
{
    [SerializeField] private GameObject playerCamera;

    public override void OnNetworkSpawn()
    {
        Debug.Log($"Client {NetworkManager.Singleton.LocalClientId} spawned player {OwnerClientId} IsOwner={IsOwner}");
        playerCamera.SetActive(IsLocalPlayer);
    }
}