using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryUIController : NetworkBehaviour
{
    private void Awake()
    {
        var scoreText = transform.Find("ScoreText").GetComponent<Text>();
        Debug.Log("Winner is " + MultiplayerGameController.Instance.winPlayer.Value);
        Debug.Log("I am " + NetworkManager.Singleton.LocalClientId);
        var isWinner = MultiplayerGameController.Instance.winPlayer.Value == NetworkManager.Singleton.LocalClientId;
        var text = "You are winner";
        if (!isWinner) text = "You've lost";
        scoreText.text = text;
    }

    public void ToMenu()
    {
        NetworkManager.Singleton.Shutdown();
        LevelController.ToMenu();
    }
}
