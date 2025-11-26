using UnityEngine;
using UnityEngine.UI;

public class SoundSettingsUI : MonoBehaviour
{
    [SerializeField] private Toggle bgmToggle;
    [SerializeField] private Toggle sfxToggle;

    private void Start()
    {
        if (SoundManager.Instance != null)
        {
            if (bgmToggle)
            {
                bgmToggle.isOn = !SoundManager.Instance.IsBgmMuted;
                bgmToggle.onValueChanged.AddListener(OnBgmToggle);
            }

            if (sfxToggle)
            {
                sfxToggle.isOn = !SoundManager.Instance.IsSfxMuted;
                sfxToggle.onValueChanged.AddListener(OnSfxToggle);
            }
        }
    }

    private void OnDestroy()
    {
        if (bgmToggle) bgmToggle.onValueChanged.RemoveListener(OnBgmToggle);
        if (sfxToggle) sfxToggle.onValueChanged.RemoveListener(OnSfxToggle);
    }

    private void OnBgmToggle(bool isOn)
    {
        SoundManager.Instance?.SetBgmMute(!isOn);
    }

    private void OnSfxToggle(bool isOn)
    {
        SoundManager.Instance?.SetSfxMute(!isOn);
    }
}
