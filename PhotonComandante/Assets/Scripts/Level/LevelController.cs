using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObservables;

public class LevelController : MonoBehaviourPunCallbacks
{    
    public static Observable<int> playerPoints = new Observable<int>() { Value = 0 };
    private static LevelController _instance;

    private void Awake()
    {
        _instance = this;
    }

    private IEnumerator OnVictory()
    {
        yield return new WaitForSeconds(1);
        PhotonNetwork.LoadLevel("VictoryScene");
    }
    
    private IEnumerator DefeatDelay()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("Scenes/DefeatScene");
    }
    
    public static void OnLevelComplete()
    {
        _instance.StartCoroutine("OnVictory");
    }

    public static void AddPoints(int points)
    {
        playerPoints.SetValue(playerPoints.Value + points);
    }

    public static void RestartLevel()
    {
        SceneManager.LoadScene("Scenes/Level1");
    }
    
    public static void ToMenu()
    {
        SceneManager.LoadScene("Scenes/MenuScene");
    }
}
