using TMPro;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text livesText;

    private void OnEnable()
    {
        hudRoot.SetActive(false);
        MainMenuController.OnGameStart += HandleGameStartHUD;
    }

    private void OnDisable()
    {
        if (GameSession.Instance == null) return;
        MainMenuController.OnGameStart -= HandleGameStartHUD;

        GameSession.Instance.OnGameStarted -= HandleGameStarted;
        GameSession.Instance.OnGameEnded -= HandleGameEnded;

        GameSession.Instance.OnScoreChanged -= HandleScoreChanged;
        GameSession.Instance.OnCoinsChanged -= HandleCoinsChanged;
        GameSession.Instance.OnLivesChanged -= HandleLivesChanged;

    }

    private void HandleGameStartHUD()
    {
        TrySubscribe();    
        hudRoot.SetActive(true);
    }


    private void TrySubscribe()
    {
        Debug.Log("TrySubscribe");

        if (GameSession.Instance == null) return;

        GameSession.Instance.OnGameStarted += HandleGameStarted;
        GameSession.Instance.OnGameEnded += HandleGameEnded;

        GameSession.Instance.OnScoreChanged += HandleScoreChanged;
        GameSession.Instance.OnCoinsChanged += HandleCoinsChanged;
        GameSession.Instance.OnLivesChanged += HandleLivesChanged;

        HandleScoreChanged(GameSession.Instance.Score);
        HandleCoinsChanged(GameSession.Instance.CoinsRun);
        HandleLivesChanged(GameSession.Instance.Lives);

        bool active = GameSession.Instance.HasRunStarted && !GameSession.Instance.IsGameOver;
        if (hudRoot) hudRoot.SetActive(active);
    }


    private void HandleGameStarted()
    {
        hudRoot.SetActive(true);
    }

    private void HandleGameEnded()
    {
        hudRoot.SetActive(false);
    }

    private void HandleScoreChanged(int value)
    {
        if (scoreText) scoreText.text = "SCORE: "+value.ToString();
    }

    private void HandleCoinsChanged(int value)
    {
        if (coinsText) coinsText.text = "COIN: "+value.ToString();
    }

    private void HandleLivesChanged(int value)
    {
        if (livesText) livesText.text = "LIVES: "+value.ToString();
    }
}
