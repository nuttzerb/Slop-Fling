using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    [SerializeField] private Button homeButton;
    [SerializeField] private Button retryButton;

    private void Awake()
    {
        if (panel) panel.SetActive(false);
    }

    private void OnEnable()
    {
        if (GameSession.Instance != null)
        {
            GameSession.Instance.OnGameOver += HandleGameOver;
        }
        if (homeButton)  homeButton.onClick.AddListener(OnClickHome);
        if (retryButton) retryButton.onClick.AddListener(OnClickRetry);
    }

    private void OnDisable()
    {
        if (GameSession.Instance != null)
            GameSession.Instance.OnGameOver -= HandleGameOver;

        if (homeButton)  homeButton.onClick.RemoveListener(OnClickHome);
        if (retryButton) retryButton.onClick.RemoveListener(OnClickRetry);
    }

    private void HandleGameOver()
    {
        if (panel) panel.SetActive(true);
        Time.timeScale = 0f;
    }

    // 🏠 HOME: về trạng thái vừa mở game
    public void OnClickHome()
    {
        Time.timeScale = 1f;

        // reset gameplay (ball idle, gameStarted = false, dọn obstacle)
        var gm = FindObjectOfType<GameplayManager>();
        if (gm) gm.ResetGameToMenu();

        // hiện lại menu + highscore/total coins
        var menu = FindObjectOfType<MainMenuController>(true);
        if (menu)
            menu.ShowMenu();

        if (panel) panel.SetActive(false);
    }

    // 🔁 RETRY: giống trạng thái vừa ấn "tap to play"
    public void OnClickRetry()
    {
        Time.timeScale = 1f;

        // chuẩn bị state gameplay về idle để start run mới
        var gm = FindObjectOfType<GameplayManager>();
        if (gm) gm.ResetGameToMenu();

        if (panel) panel.SetActive(false);

        // Gọi lại đúng flow "bắt đầu game" như tap-to-play / nút Play
        MainMenuController.TriggerGameStartFromOutside();
    }
}
