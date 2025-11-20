using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    private bool gameStarted = false;

    [SerializeField] private BallController ball;

    private void OnEnable()
    {
        MainMenuController.OnGameStart += HandleGameStart;
    }

    private void OnDisable()
    {
        MainMenuController.OnGameStart -= HandleGameStart;
    }

    private void Start()
    {
        // Chặn gameplay ngay khi vào scene
        gameStarted = false;

        if (ball)
            ball.SetIdleState();
    }

    private void HandleGameStart()
    {
        if (gameStarted) return;

        gameStarted = true;

        Debug.Log("GAME START!");

        if (ball)
            ball.BeginGameplay();
    }
}
