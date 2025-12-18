using Photon.Pun;
using UnityEngine;

public class PlayerCameraController : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerCamera;

    private PhotonView _photonView;

    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (_photonView == null) _photonView = GetComponent<PhotonView>();
        if (playerCamera != null) playerCamera.SetActive(_photonView != null && _photonView.IsMine);
    }
}