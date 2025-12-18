using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObservables;

public class LevelController : MonoBehaviour
{
    
    public static Observable<int> playerPoints = new Observable<int>() { Value = 0 };
    private static LevelController _instance;

    public static int currentEnemiesCount = 0;
    public static Observable<int> currentWave = new Observable<int>() { Value = 0 };
    public static int enemiesCount = 5;
    
    public GameObject enemyPrefab;
    public List<Transform> spawnPoint;

    public static String playerName;

    private static FirebaseService service = new FirebaseService();

    private void Awake()
    {
        Debug.Log("STARTED");
        _instance = this;
        currentEnemiesCount = 0;
        enemiesCount = 5;
        currentWave.Value = 0;
        OnNextWave();
    }

    private void Start()
    {
        
    }

    private IEnumerator OnVictory()
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("Scenes/VictoryScene");
    }
    
    private IEnumerator DefeatDelay()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("Scenes/DefeatScene");
        var score = new PlayerScore();
        score.Name = playerName;
        score.Score = playerPoints.Value;
        service.SaveScoreAsync(score);
    }
    
    public static void OnLevelComplete()
    {
        _instance.StartCoroutine("OnVictory");
    }
    
    public static void OnDefeat()
    {
        _instance.StartCoroutine("DefeatDelay");
    }

    public static void AddPoints(int points)
    {
        playerPoints.SetValue(playerPoints.Value + points);
        Debug.Log(playerPoints.Value);
    }

    public static void RestartLevel()
    {
        // SceneManager.LoadScene("Scenes/Level1");
        // currentEnemiesCount = 0;
        // enemiesCount = 5;
        // currentWave.Value = 0;
        // OnNextWave();
    }
    
    public static void ToMenu()
    {
        SceneManager.LoadScene("Scenes/MenuScene");
    }

    public static void OnEnemyDied()
    {
        currentEnemiesCount--;
        if (currentEnemiesCount == 0)
        {
            OnNextWave();
        }
    }

    public static void OnNextWave()
    {
        currentWave.Value++;
        enemiesCount = (int)(enemiesCount * 1.2);
        currentEnemiesCount = enemiesCount;

        for (int i = 0; i < enemiesCount; i++)
        {
            Transform randomSpawn = _instance.spawnPoint[UnityEngine.Random.Range(0, _instance.spawnPoint.Count)];
            Instantiate(_instance.enemyPrefab, randomSpawn.position, randomSpawn.rotation);
        }
    }

}
