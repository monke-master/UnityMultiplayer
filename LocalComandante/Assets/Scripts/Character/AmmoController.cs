using System;
using Unity.Netcode;
using UnityEngine;

public class AmmoController : NetworkBehaviour
{

    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform shotPoint;
    [SerializeField] private float bulletSpeed;

    private StateHolder _stateHolder;

    private void Awake()
    {
        _stateHolder = GetComponent<StateHolder>();
    }

    public void Shoot()
    {
        if (IsOwner)
        {
            if (_stateHolder?.clipAmmo != null && _stateHolder.clipAmmo.Value > 0)
            {
                ShootServerRpc(_stateHolder.direction);
                _stateHolder.clipAmmo.Value--;
            }
            else
            {
                _stateHolder.movementState.Value = (State.Aim);
            }
        }
    }
    
    [ServerRpc]
    public void ShootServerRpc(float direction)
    {
        var obj = Instantiate(bullet, shotPoint.position, Quaternion.identity);
        var rb = obj.GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(bulletSpeed * direction, 0f);
    
        var netObj = obj.GetComponent<NetworkObject>();
        netObj.Spawn();

        
    }


    public void OnRechargeCompleted()
    {
        var availableAmmo = Math.Min(StateHolder.CLIP_CAPACITY, _stateHolder.ammoCount.Value);
        _stateHolder.clipAmmo.Value = (availableAmmo);
        _stateHolder.ammoCount.Value = (_stateHolder.ammoCount.Value - availableAmmo);
        _stateHolder.onRechargeCompleted?.Invoke();
    }

    public void AddAmmo(int ammoCount)
    {
        _stateHolder.ammoCount.Value = (_stateHolder.ammoCount.Value + ammoCount);
    }
}
