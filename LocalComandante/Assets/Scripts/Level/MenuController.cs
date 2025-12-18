using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject menuCanvas;
    [SerializeField] private GameObject settingsCanvas;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _soundsSlider;

    private void Awake()
    {
        _soundsSlider.onValueChanged.AddListener((v) =>
        {
            AudioController.SetSoundsVolume(v);
        });
        
        _musicSlider.onValueChanged.AddListener((v) =>
        {
            AudioController.SetMusicVolume(v);
        });
    }

    public void ToSettings()
    {
        menuCanvas.SetActive(false);
        settingsCanvas.SetActive(true);
    }
    
    public void ToMenu()
    {
        settingsCanvas.SetActive(false);
        menuCanvas.SetActive(true);
    }
    
    public void ThreePlayers()
    {
        SceneManager.LoadScene("Scenes/ConnectScene");
    }

    public void ToLevel()
    {
        SceneManager.LoadScene("Scenes/Level1");
    }

    public void Quit()
    {
        Application.Quit();
    }
    
}
