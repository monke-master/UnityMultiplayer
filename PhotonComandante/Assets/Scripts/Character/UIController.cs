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
        _clipAmmoText = transform.Find("ClipAmmoText").GetComponent<Text>();
        _ammoText = transform.Find("AmmoText").GetComponent<Text>();
        _healthText = transform.Find("HealthText").GetComponent<Text>();
        _pauseCanvas = transform.Find("Pause").gameObject;

        // Подписка на паузу
        playerInput.onPause += () =>
        {
            _pauseCanvas.GetComponent<PauseController>().Show();
        };

        // Инициализация UI текущими значениями
        UpdateClipAmmoText(stateHolder.clipAmmo);
        UpdateAmmoText(stateHolder.ammoCount);
        UpdateHealthText(stateHolder.health);
    }

    private void Update()
    {
        // Простая поллинговая синхронизация для локального владельца
        UpdateClipAmmoText(stateHolder.clipAmmo);
        UpdateAmmoText(stateHolder.ammoCount);
        UpdateHealthText(stateHolder.health);
    }

    private void UpdateClipAmmoText(int value)
    {
        _clipAmmoText.text = $"Ammo in clip: {value}/{StateHolder.CLIP_CAPACITY}";
    }

    private void UpdateAmmoText(int value)
    {
        _ammoText.text = $"Ammo: {value}";
    }

    private void UpdateHealthText(float value)
    {
        _healthText.text = $"Health: {Math.Max(value, 0)}/{stateHolder.MAX_HEALTH}";
    }
}