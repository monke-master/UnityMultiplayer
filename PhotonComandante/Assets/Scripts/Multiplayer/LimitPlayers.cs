using Photon.Pun;
using UnityEngine;

public class LimitPlayers : MonoBehaviour
{
    [SerializeField] private byte maxPlayers = 2;

    private void Start()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null)
        {
            PhotonNetwork.CurrentRoom.MaxPlayers = maxPlayers;
        }
    }
}