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
    private Text _scoreText;
    private GameObject _pauseCanvas;
    private Text _waveText;

    private void Awake()
    {
        _clipAmmoText = transform.Find("ClipAmmoText").gameObject.GetComponent<Text>();
        _ammoText = transform.Find("AmmoText").gameObject.GetComponent<Text>();
        _healthText = transform.Find("HealthText").gameObject.GetComponent<Text>();
        _scoreText = transform.Find("PointsText").gameObject.GetComponent<Text>();
        _pauseCanvas = transform.Find("Pause").gameObject;
        _waveText = transform.Find("WaveText").gameObject.GetComponent<Text>();
        
        _clipAmmoText.text = "В обойме: " + stateHolder.clipAmmo.Value + "/" + StateHolder.CLIP_CAPACITY;
        _ammoText.text = "Патроны: " + stateHolder.ammoCount.Value;
        _healthText.text = "Здоровье: " + stateHolder.health.Value + "/" + stateHolder.MAX_HEALTH;
        _scoreText.text = "Очки: " + LevelController.playerPoints.Value;
        _waveText.text = "Волна: " + LevelController.currentWave.Value;

        playerInput.onPause += () =>
        {
            _pauseCanvas.GetComponent<PauseController>().Show();
        };
    }

    void Start()
    {
        stateHolder.clipAmmo.OnChanged += () =>
        {
            _clipAmmoText.text = "В обойме: " + stateHolder.clipAmmo.Value + "/" + StateHolder.CLIP_CAPACITY;
        };
        
        stateHolder.ammoCount.OnChanged += () =>
        {
            _ammoText.text = "Патроны: " + stateHolder.ammoCount.Value;
        };
        
        stateHolder.health.OnChanged += () =>
        {
            _healthText.text = "Здоровье: " + Math.Max(stateHolder.health.Value, 0) + "/" + stateHolder.MAX_HEALTH;
        };

        LevelController.playerPoints.OnChanged += () =>
        {
            _scoreText.text = "Очки: " + LevelController.playerPoints.Value;
        };

        LevelController.currentWave.OnChanged += () =>
        {
            if (_waveText != null)
            {
                _waveText.text = "Волна: " + LevelController.currentWave.Value;
            }
        };
    }
}
