using System;
using UnityEngine;

public class AmmoController : MonoBehaviour
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
        var obj = Instantiate(bullet, shotPoint.position, Quaternion.identity);
        var rigidbody = obj.GetComponent<Rigidbody2D>();
        rigidbody.velocity = new Vector2(bulletSpeed * _stateHolder.direction, shotPoint.position.y);

        _stateHolder.clipAmmo.SetValue(_stateHolder.clipAmmo.Value - 1);
    }

    public void OnRechargeCompleted()
    {
        var availableAmmo = Math.Min(StateHolder.CLIP_CAPACITY, _stateHolder.ammoCount.Value);
        _stateHolder.clipAmmo.SetValue(availableAmmo);
        _stateHolder.ammoCount.SetValue(_stateHolder.ammoCount.Value - availableAmmo);
        _stateHolder.onRechargeCompleted?.Invoke();
    }

    public void AddAmmo(int ammoCount)
    {
        _stateHolder.ammoCount.SetValue(_stateHolder.ammoCount.Value + ammoCount);
    }
}
