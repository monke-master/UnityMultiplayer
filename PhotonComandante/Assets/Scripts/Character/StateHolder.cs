using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;
using UnityObservables;

public class StateHolder : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Movement & Action States")]
    public State movementState = State.Idle;
    public bool shot = false;

    [Header("Direction & Health")]
    public int direction = 1;
    public int MAX_HEALTH = 100;
    public float health = 100f;

    [Header("Ammo")]
    public const int CLIP_CAPACITY = 30;
    public const int INIT_AMMO = 100;
    public int clipAmmo = CLIP_CAPACITY;
    public int ammoCount = INIT_AMMO;

    [Header("Local Events")]
    public Action onRecharge;
    public Action onRechargeCompleted;
    public Action onAttack;
    
    public long clientId = 0;

    private PhotonView _photonView;

    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
        if (_photonView != null)
        {
            clientId = PhotonNetwork.LocalPlayer.ActorNumber;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext((int)movementState);
            stream.SendNext(shot);
            stream.SendNext(direction);
            stream.SendNext(health);
            stream.SendNext(clipAmmo);
            stream.SendNext(ammoCount);
            // Конвертируем ulong в long для сериализации (Photon не поддерживает ulong)
            stream.SendNext((long)clientId);
        }
        else
        {
            movementState = (State)(int)stream.ReceiveNext();
            shot = (bool)stream.ReceiveNext();
            direction = (int)stream.ReceiveNext();
            health = (float)stream.ReceiveNext();
            clipAmmo = (int)stream.ReceiveNext();
            ammoCount = (int)stream.ReceiveNext();
            // Конвертируем обратно из long в ulong
            clientId =  (long)stream.ReceiveNext();
        }
    }
}