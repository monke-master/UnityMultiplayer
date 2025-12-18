using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject menuCanvas;
    [SerializeField] private GameObject settingsCanvas;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _soundsSlider;

    private PersistentNetwork _lobby;

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

        _lobby = FindObjectOfType<PersistentNetwork>();
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
    
    public void Host()
    {
        // Создаем комнату - после создания произойдет переход на HostScene через OnJoinedRoom
        if (_lobby == null) _lobby = FindObjectOfType<PersistentNetwork>();
        if (_lobby != null)
        {
            _lobby.CreateRoom(_lobby.defaultRoomName);
        }
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
