using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonClickSfx : MonoBehaviour
{
    [SerializeField] private SoundId soundId = SoundId.UI_Click;

    private void Awake()
    {
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    private void OnDestroy()
    {
        var btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.RemoveListener(OnClick);
    }

    private void OnClick()
    {
        SoundManager.Instance?.PlaySfx(soundId);
    }
}
