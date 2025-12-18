using System;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private StateHolder stateHolder;
    [SerializeField] private PlayerInputController playerInput;
    
    private Text _clipAmmoText;
    private Text _ammoText;
    private Text _healthText;
    private GameObject _pauseCanvas;

    private void Awake()
    {
        _clipAmmoText = transform.Find("ClipAmmoText").gameObject.GetComponent<Text>();
        _ammoText = transform.Find("AmmoText").gameObject.GetComponent<Text>();
        _healthText = transform.Find("HealthText").gameObject.GetComponent<Text>();
        _pauseCanvas = transform.Find("Pause").gameObject;
        
        _clipAmmoText.text = "Ammo in clip: " + stateHolder.clipAmmo.Value + "/" + StateHolder.CLIP_CAPACITY;
        _ammoText.text = "Ammo: " + stateHolder.ammoCount.Value;
        _healthText.text = "Health: " + stateHolder.health.Value + "/" + stateHolder.MAX_HEALTH;

        playerInput.onPause += () =>
        {
            _pauseCanvas.GetComponent<PauseController>().Show();
        };
    }

    void Start()
    {
        stateHolder.clipAmmo.OnChanged += () =>
        {
            _clipAmmoText.text = "Ammo in clip: " + stateHolder.clipAmmo.Value + "/" + StateHolder.CLIP_CAPACITY;
        };
        
        stateHolder.ammoCount.OnChanged += () =>
        {
            _ammoText.text = "Ammo: " + stateHolder.ammoCount.Value;
        };
        
        stateHolder.health.OnChanged += () =>
        {
            _healthText.text = "Health: " + Math.Max(stateHolder.health.Value, 0) + "/" + stateHolder.MAX_HEALTH;
        };
    }
}
