using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LeaderboardController : MonoBehaviour
{

    public Transform content;
    public GameObject textPrefab;
    public GameObject sectionHeaderPrefab;
    
    private FirebaseService service = new();

    private async void Awake()
    {
        var combined = await service.LoadLeaderboardWithPlayerAsync(LevelController.playerName);
        ShowLeaderboard(combined, LevelController.playerName);
    }

    public void ShowLeaderboard(List<PlayerScore> scores, string currentPlayer)
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);

        // Заголовок Top-10
        Instantiate(sectionHeaderPrefab, content).GetComponentInChildren<Text>().text = "🏆 TOP-10";

        int count = 0;
        foreach (var p in scores.Take(10))
        {
            CreateRow(p, count, currentPlayer);
            count++;
        }

        // Заголовок "Около тебя"
        Instantiate(sectionHeaderPrefab, content).GetComponentInChildren<Text>().text = "📍 Around You";

        int startIndex = 10; // после топа
        foreach (var p in scores.Skip(10))
        {
            CreateRow(p, startIndex, currentPlayer);
            startIndex++;
        }
    }

    private void CreateRow(PlayerScore player, int position, string currentPlayer)
    {
        var row = Instantiate(textPrefab, content);
        var texts = row.GetComponentsInChildren<Text>();
        
        texts[1].text = player.Name;
        texts[0].text = player.Score.ToString();
    }

    public void GoBack()
    {
        SceneManager.LoadScene("Scenes/MenuScene");
    }
}
