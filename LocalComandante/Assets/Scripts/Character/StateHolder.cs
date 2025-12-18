
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityObservables;

public class StateHolder : NetworkBehaviour
{
    [Header("Movement & Action States")]
    public NetworkVariable<State> movementState = new NetworkVariable<State>(State.Idle, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> shot = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Direction & Health")]
    public int direction = 1;
    public int MAX_HEALTH = 100;
    public NetworkVariable<float> health = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Ammo")]
    public const int CLIP_CAPACITY = 30;
    public const int INIT_AMMO = 100;
    public NetworkVariable<int> clipAmmo = new NetworkVariable<int>(CLIP_CAPACITY, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> ammoCount = new NetworkVariable<int>(INIT_AMMO, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Local Events")]
    public Action onRecharge;
    public Action onRechargeCompleted;
    public Action onAttack;
    
    public NetworkVariable<ulong> clientId = new NetworkVariable<ulong>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
}