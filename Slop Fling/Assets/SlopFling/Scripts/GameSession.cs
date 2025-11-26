using System;
using UnityEngine;
using UnityEngine.SceneManagement;
[DefaultExecutionOrder(-100)]
public class GameSession : MonoBehaviour
{
    public event Action OnGameStarted;
    public event Action OnGameEnded;

    public static GameSession Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private GameDefinition _gameDefinition;
    [SerializeField] private int startingLives = 3;

    public int Score { get; private set; }
    public int TotalCoins { get; private set; }
    public int Lives { get; private set; }
    public bool IsGameOver { get; private set; }
    public int CoinsRun { get; private set; }
    public bool HasRunStarted { get; private set; }

    public event Action<int> OnScoreChanged;
    public event Action<int> OnCoinsChanged;
    public event Action<int> OnLivesChanged;
    public event Action OnGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Nếu muốn giữ qua nhiều scene:
        // DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Lives = startingLives;
        TotalCoins = PlayerPrefs.GetInt($"{_gameDefinition.gameId}_Coins", 0);

        OnScoreChanged?.Invoke(Score);
        OnCoinsChanged?.Invoke(TotalCoins);
        OnLivesChanged?.Invoke(Lives);
    }
    public void StartRun()
    {
        Debug.Log("StartRun");
        HasRunStarted = true;
        IsGameOver = false;

        Score = 0;
        CoinsRun = 0;
        Lives = startingLives;

        OnScoreChanged?.Invoke(Score);
        OnCoinsChanged?.Invoke(CoinsRun);
        OnLivesChanged?.Invoke(Lives);

        OnGameStarted?.Invoke();
    }


    public void HandleObstacleHit(Obstacle obstacle)
    {
        if (IsGameOver || obstacle == null) return;

        switch (obstacle.type)
        {
            case ObstacleType.Coin:
                AddCoins(10);
                break;

            case ObstacleType.Brick:
                AddScore(15);
                break;

            case ObstacleType.Block:
                TakeHit(1);
                break;
        }

        obstacle.OnHitVisual();
    }

    private void AddScore(int amount)
    {
        Score += amount;
        OnScoreChanged?.Invoke(Score);
    }

    private void AddCoins(int amount)
    {
        TotalCoins += amount;
        CoinsRun += amount;
        OnCoinsChanged?.Invoke(CoinsRun);

        PlayerPrefs.SetInt($"{_gameDefinition.gameId}_Coins", TotalCoins);
        PlayerPrefs.Save();
    }

    private void TakeHit(int amount)
    {
        Lives -= amount;
        if (Lives < 0) Lives = 0;

        OnLivesChanged?.Invoke(Lives);

        if (Lives <= 0)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        HasRunStarted = false;

        // Lưu high score
        string hsKey = $"{_gameDefinition.gameId}_HighScore";
        int oldHS = PlayerPrefs.GetInt(hsKey, 0);
        if (Score > oldHS)
        {
            PlayerPrefs.SetInt(hsKey, Score);
        }
        PlayerPrefs.Save();

        OnGameOver?.Invoke();
        OnGameEnded?.Invoke();
    }
    public void GameOverFromFall()
    {
        Lives = 0;
        OnLivesChanged?.Invoke(Lives);
        TriggerGameOver();
    }
    // Gọi từ UI
    public void Retry()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    public void BackToMenu(string menuSceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}
