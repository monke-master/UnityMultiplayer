using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryUIController : MonoBehaviour
{
    private void Awake()
    {
        var scoreText = transform.Find("ScoreText").GetComponent<Text>();
        scoreText.text = "Player " + MultiplayerGameController.victoryPlayer + " won";
    }

    public void ToMenu()
    {
        LevelController.ToMenu();
    }
}
