using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkinShopUI : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private SkinDatabase skinDb;
    [SerializeField] private GameDefinition gameDefinition;

    [Header("UI - Images")]
    [SerializeField] private Image leftIcon;
    [SerializeField] private Image centerIcon;
    [SerializeField] private Image rightIcon;

    [Header("UI - Buttons")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button buyButton;

    [Header("UI - Overlay / Text")]
    [SerializeField] private GameObject lockOverlayRoot;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text skinNameText;

    private const string PREF_SKIN_UNLOCK_PREFIX = "SF_SkinUnlocked_";
    private const string PREF_CURRENT_SKIN = "SF_CurrentSkinId";

    private int _currentIndex = 0;

    private void Awake()
    {
        // hook button
        if (prevButton) prevButton.onClick.AddListener(OnPrev);
        if (nextButton) nextButton.onClick.AddListener(OnNext);
        if (buyButton) buyButton.onClick.AddListener(OnBuy);
    }

    private void OnDestroy()
    {
        if (prevButton) prevButton.onClick.RemoveListener(OnPrev);
        if (nextButton) nextButton.onClick.RemoveListener(OnNext);
        if (buyButton) buyButton.onClick.RemoveListener(OnBuy);
    }

    private void Start()
    {
        if (skinDb == null || skinDb.Count == 0)
        {
            Debug.LogWarning("[SkinShopUI] skinDb chưa gán hoặc rỗng.");
            return;
        }

        string savedId = PlayerPrefs.GetString(PREF_CURRENT_SKIN, string.Empty);
        int idx = skinDb.IndexOf(savedId);

        if (idx < 0)
        {
            idx = 0;
            for (int i = 0; i < skinDb.Count; i++)
            {
                var s = skinDb.GetByIndex(i);
                if (s != null && s.unlockedByDefault)
                {
                    idx = i;
                    break;
                }
            }
        }

        _currentIndex = idx;
        RefreshCoinsText();
        RefreshUI();
    }

    // ======= COINS =======

    private int GetTotalCoins()
    {
        return PlayerPrefs.GetInt($"{gameDefinition.gameId}_Coins", 0);
    }

    private void SetTotalCoins(int value)
    {
        PlayerPrefs.SetInt($"{gameDefinition.gameId}_Coins", value);
        PlayerPrefs.Save();
        RefreshCoinsText();
    }

    private void RefreshCoinsText()
    {
        if (coinsText)
            coinsText.text = GetTotalCoins().ToString();
    }

    // ======= UNLOCK STATE =======

    private bool IsUnlocked(SkinDatabase.SkinItem skin)
    {
        if (skin == null) return false;
        if (skin.unlockedByDefault) return true;

        string key = PREF_SKIN_UNLOCK_PREFIX + skin.skinId;
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    private void SetUnlocked(SkinDatabase.SkinItem skin)
    {
        if (skin == null) return;
        string key = PREF_SKIN_UNLOCK_PREFIX + skin.skinId;
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }

    // ======= EQUIP =======

    private void EquipSkin(SkinDatabase.SkinItem skin)
    {
        if (skin == null) return;
        if (!IsUnlocked(skin)) return;

        PlayerPrefs.SetString(PREF_CURRENT_SKIN, skin.skinId);
        PlayerPrefs.Save();

        // Nếu BallSkinController đang trong scene, sync luôn
        var ballSkin = FindObjectOfType<BallSkinController>();
        if (ballSkin != null)
        {
            ballSkin.ApplyCurrentSkin();
        }
    }

    // ======= SLIDER =======

    private void OnPrev()
    {
        if (skinDb == null || skinDb.Count == 0) return;
        _currentIndex = (_currentIndex - 1 + skinDb.Count) % skinDb.Count;
        RefreshUI();
        SoundManager.Instance?.PlaySfx(SoundId.UI_Click);
    }

    private void OnNext()
    {
        if (skinDb == null || skinDb.Count == 0) return;
        _currentIndex = (_currentIndex + 1) % skinDb.Count;
        RefreshUI();
        SoundManager.Instance?.PlaySfx(SoundId.UI_Click);
    }

    private void OnBuy()
    {
        var skin = skinDb.GetByIndex(_currentIndex);
        if (skin == null) return;
        if (IsUnlocked(skin)) return;

        int coins = GetTotalCoins();
        if (coins < skin.price)
        {
            SoundManager.Instance?.PlaySfx(SoundId.Sfx_HitBlock);
            return;
        }

        coins -= skin.price;
        SetTotalCoins(coins);

        SetUnlocked(skin);
        EquipSkin(skin);

        var menu = FindObjectOfType<MainMenuController>();
        if (menu) menu.LoadUIValues();

        SoundManager.Instance?.PlaySfx(SoundId.UI_Click);
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (skinDb == null || skinDb.Count == 0) return;

        int count = skinDb.Count;
        int leftIndex = (_currentIndex - 1 + count) % count;
        int rightIndex = (_currentIndex + 1) % count;

        var leftSkin = skinDb.GetByIndex(leftIndex);
        var centerSkin = skinDb.GetByIndex(_currentIndex);
        var rightSkin = skinDb.GetByIndex(rightIndex);

        if (leftIcon) leftIcon.sprite = leftSkin.icon;
        if (centerIcon) centerIcon.sprite = centerSkin.icon;
        if (rightIcon) rightIcon.sprite = rightSkin.icon;

        bool unlocked = IsUnlocked(centerSkin);

        if (lockOverlayRoot) lockOverlayRoot.SetActive(!unlocked);
        if (buyButton) buyButton.gameObject.SetActive(!unlocked);

        if (priceText)
        {
            bool showPrice = !unlocked && centerSkin != null;
            priceText.gameObject.SetActive(showPrice);
            priceText.text = showPrice ? centerSkin.price.ToString() : "";
        }

        if (unlocked)
            EquipSkin(centerSkin);
        if (skinNameText)
            skinNameText.text = centerSkin.skinName;
    }

}
