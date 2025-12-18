using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class FirebaseService
{
    private string projectUrl = "https://comandante-d1302-default-rtdb.firebaseio.com/";

    private string LeaderboardUrl => $"{projectUrl}leaderboard.json";
    
    public async UniTask SaveScoreAsync(PlayerScore player, CancellationToken token = default)
    {
        var scores = await LoadScoresAsync();
        var currentPlayer = scores.FindIndex(p => p.Name == player.Name);
        
        if (currentPlayer != -1 && scores[currentPlayer].Score >= player.Score) return;
        
        string json = JsonConvert.SerializeObject(player);
        string url = $"{projectUrl}leaderboard/{player.Name}.json";

        using var request = new UnityWebRequest(url, "PUT");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        await request.SendWebRequest().ToUniTask(cancellationToken: token);

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"✅ Score saved for {player.Name}");
        }
        else
        {
            Debug.LogError($"❌ Error saving score: {request.error}");
        }
    }
    
    public async UniTask<List<PlayerScore>> LoadScoresAsync(CancellationToken token = default)
    {
        using var request = UnityWebRequest.Get(LeaderboardUrl);
        await request.SendWebRequest().ToUniTask(cancellationToken: token);

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"❌ Error loading scores: {request.error}");
            return new List<PlayerScore>();
        }

        string json = request.downloadHandler.text;
        if (string.IsNullOrEmpty(json) || json == "null")
            return new List<PlayerScore>();
    
        var dict = JsonConvert.DeserializeObject<Dictionary<string, PlayerScore>>(json);
        if (dict == null)
            return new List<PlayerScore>();

        var scores = new List<PlayerScore>(dict.Values);
        Debug.Log(dict.Values);
        scores.Sort((a, b) => b.Score.CompareTo(a.Score));

        return scores;
    }
    
    public async UniTask<List<PlayerScore>> LoadLeaderboardWithPlayerAsync(string playerName, CancellationToken token = default)
    {
        var scores = await LoadScoresAsync(token);
        
        var top10 = scores.Take(10).ToList();
        
        int index = scores.FindIndex(p => p.Name == playerName);

        List<PlayerScore> playerWindow = new();
        if (index != -1)
        {
            int start = Mathf.Max(0, index - 3);
            int end = Mathf.Min(scores.Count - 1, index + 3);
            playerWindow = scores.GetRange(start, end - start + 1);
        }
        else
        {
            var newPlayer = new PlayerScore { Name = playerName, Score = 0 };
            playerWindow.Add(newPlayer);
        }
        
        var combined = new List<PlayerScore>();

        foreach (var p in top10.Concat(playerWindow))
        {
            combined.Add(p);
        }

        return combined;
    }


}

[System.Serializable]
public class PlayerScore
{
    [JsonProperty("name")] public string Name;
    [JsonProperty("score")] public int Score;
}