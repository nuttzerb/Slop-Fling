using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;

public class MainMenuController : MonoBehaviour
{
    public static event Action OnGameStart; // For single-scene games

    [Header("Config")]
    [SerializeField] private GameDefinition gameDef;

    [Header("UI References")]
    [SerializeField] private CanvasGroup menuRoot;
    [SerializeField] private GameObject tapToPlayLayer;
    [SerializeField] private GameObject skinSliderRoot;
    [SerializeField] private Button playButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text currencyText;

    [SerializeField] private Image logoImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button closeSettingsButton;

    [SerializeField] private GameObject settingPanel;

    private void Awake()
    {
        ApplyConfig();
        SetupStartModeUI();
        HookButtons();
        LoadUIValues();
    }
    private void OnDisable()
    {
        if (settingsButton) settingsButton.onClick.RemoveListener(() => TogglePanel(settingPanel, true));
        if (closeSettingsButton) closeSettingsButton.onClick.RemoveListener(() => TogglePanel(settingPanel, false));

    }
    private void ApplyConfig()
    {
        if (gameDef == null) return;

        if (logoImage) logoImage.sprite = gameDef.logo;
        if (backgroundImage) backgroundImage.sprite = gameDef.background;

        if (shopButton) shopButton.gameObject.SetActive(gameDef.hasShop);
        if (skinSliderRoot) skinSliderRoot.SetActive(gameDef.hasSkin);

    }
    public static void TriggerGameStartFromOutside()
    {
        OnGameStart?.Invoke();
    }

    public void ShowMenu()
    {
        if (menuRoot)
        {
            menuRoot.alpha = 1f;
            menuRoot.blocksRaycasts = true;
            menuRoot.interactable = true;
        }

        if (tapToPlayLayer && gameDef.startMode == GameStartMode.SingleSceneTapToPlay)
        {
            tapToPlayLayer.SetActive(true);
        }
        SoundManager.Instance?.PlayBgm(SoundId.Bgm_Menu, true);

        LoadUIValues();
    }
    public void LoadUIValues()
    {
        if (!gameDef) return;

        int hs = PlayerPrefs.GetInt($"{gameDef.gameId}_HighScore", 0);
        if (highScoreText) highScoreText.text = $"HighScore : {hs}";

        int coins = PlayerPrefs.GetInt($"{gameDef.gameId}_Coins", 0);
        if (currencyText) currencyText.text = $"Total Coins: {coins}";
    }

    private void SetupStartModeUI()
    {
        if (tapToPlayLayer) tapToPlayLayer.SetActive(false);

        switch (gameDef.startMode)
        {
            case GameStartMode.LoadScene:
                if (playButton) playButton.gameObject.SetActive(true);
                break;

            case GameStartMode.SingleSceneButton:
                if (playButton) playButton.gameObject.SetActive(true);
                break;

            case GameStartMode.SingleSceneTapToPlay:
                if (playButton) playButton.gameObject.SetActive(false);
                if (tapToPlayLayer) tapToPlayLayer.SetActive(true);
                var tapBtn = tapToPlayLayer.GetComponent<Button>();
                if (tapBtn) tapBtn.onClick.AddListener(StartGameInSameScene);
                break;
        }
    }

    private void HookButtons()
    {
        if (playButton) playButton.onClick.AddListener(OnPlayClicked);
        //  if (shopButton)    shopButton.onClick.AddListener(() => TogglePanel<ShopPanel>(true));
        if (settingsButton) settingsButton.onClick.AddListener(() => TogglePanel(settingPanel, true));
        if (closeSettingsButton) closeSettingsButton.onClick.AddListener(() => TogglePanel(settingPanel, false));

        if (quitButton) quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnPlayClicked()
    {
        switch (gameDef.startMode)
        {
            case GameStartMode.LoadScene:
                SceneManager.LoadScene(gameDef.gameplaySceneName);
                break;

            case GameStartMode.SingleSceneButton:
                StartGameInSameScene();
                break;
        }
    }

    private void StartGameInSameScene()
    {
        if (menuRoot)
        {
            menuRoot.alpha = 0f;
            menuRoot.blocksRaycasts = false;
            menuRoot.interactable = false;
        }

        if (tapToPlayLayer) tapToPlayLayer.SetActive(false);

        OnGameStart?.Invoke();
    }
    private void TogglePanel(GameObject gameObject, bool state)
    {
        if (gameObject) gameObject.SetActive(state);
    }

    private void TogglePanel<T>(bool state) where T : MonoBehaviour
    {
        var panel = FindObjectOfType<T>(true);
        if (panel) panel.gameObject.SetActive(state);
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
