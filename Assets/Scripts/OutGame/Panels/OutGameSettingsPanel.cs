using UnityEngine;
using UnityEngine.UI;

public class OutGameSettingsPanel : UIPopup
{
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(Hide);
        bgmSlider.onValueChanged.AddListener(v => SoundManager.Instance.SetBgmVolume(v));
        sfxSlider.onValueChanged.AddListener(v => SoundManager.Instance.SetSfxVolume(v));
    }

    public override void Show()
    {
        bgmSlider.SetValueWithoutNotify(SoundManager.Instance.BgmVolume);   // TongTong SoundManager는 PascalCase (이식 개조)
        sfxSlider.SetValueWithoutNotify(SoundManager.Instance.SfxVolume);
        base.Show();
    }
}
