using System;
using Photon.Pun;
using UnityEngine;

public class AmmoController : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform shotPoint;
    [SerializeField] private float bulletSpeed;

    private StateHolder _stateHolder;
    private PhotonView _photonView;

    private void Awake()
    {
        _stateHolder = GetComponent<StateHolder>();
        _photonView = GetComponent<PhotonView>();
    }

    public void Shoot()
    {
        if (_photonView == null || !_photonView.IsMine) return;

        if (_stateHolder != null && _stateHolder.clipAmmo > 0)
        {
            SpawnBullet(_stateHolder.direction);
            _stateHolder.clipAmmo--;
        }
        else
        {
            _stateHolder.movementState = State.Aim;
        }
    }

    private void SpawnBullet(float direction)
    {
        var obj = PhotonNetwork.Instantiate(bullet.name, shotPoint.position, Quaternion.identity);
        var bulletPhotonView = obj.GetComponent<PhotonView>();
        Debug.Log("bullet: " + direction);
        
        if (bulletPhotonView != null && _photonView != null)
        {
            int shooterActorNumber = _photonView.OwnerActorNr;
            Vector2 bulletVelocity = new Vector2(bulletSpeed * direction, 0f);
            
            // Устанавливаем скорость и стрелка через RPC на всех клиентах
            // Используем небольшую задержку чтобы убедиться что объект инициализирован
            StartCoroutine(SetBulletPropertiesDelayed(bulletPhotonView, bulletVelocity, shooterActorNumber));
            
            Debug.Log($"[AmmoController] Spawned bullet with velocity: {bulletVelocity}, direction: {direction}");
        }
        else
        {
            // Fallback: устанавливаем скорость локально если нет PhotonView
            var rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = new Vector2(bulletSpeed * direction, 0f);
                rb.gravityScale = 0f;
            }
        }
    }
    
    private System.Collections.IEnumerator SetBulletPropertiesDelayed(PhotonView bulletPhotonView, Vector2 velocity, int shooterActorNumber)
    {
        // Ждем один кадр чтобы убедиться что объект полностью инициализирован
        yield return null;
        
        if (bulletPhotonView != null)
        {
            // Устанавливаем скорость напрямую на Rigidbody2D
            var rb = bulletPhotonView.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = velocity;
            }
            
            bulletPhotonView.RPC(nameof(Bullet.SetShooter), RpcTarget.All, shooterActorNumber);
        }
    }

    public void OnRechargeCompleted()
    {
        if (_stateHolder == null) return;
        var availableAmmo = Math.Min(StateHolder.CLIP_CAPACITY, _stateHolder.ammoCount);
        _stateHolder.clipAmmo = availableAmmo;
        _stateHolder.ammoCount = _stateHolder.ammoCount - availableAmmo;
        _stateHolder.onRechargeCompleted?.Invoke();
    }

    public void AddAmmo(int ammoCount)
    {
        if (_stateHolder == null) return;
        _stateHolder.ammoCount = _stateHolder.ammoCount + ammoCount;
    }
}


