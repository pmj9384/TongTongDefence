using UnityEngine;
using TMPro;
using UnityEngine.UI;

// 플레이어 HP 게이지 — 값 갱신만. Slider/텍스트는 BottomBar 존에 에디터 조립 (표준 UGUI Slider, 핸들 없는 게이지).
// 색·크기·위치는 전부 Inspector 소관.
public class PlayerHpBar : UIElement
{
    [Header("씬 참조 (BottomBar 아래)")]
    [SerializeField] private Slider hpSlider;   // interactable 꺼진 게이지
    [SerializeField] private TMP_Text hpText;

    private void Start()   // 매니저 Initialize 완료 후 (관례)
    {
        gameManager.PlayerManager.Health.OnChanged += Refresh;
        Refresh(gameManager.PlayerManager.Health.Current, gameManager.PlayerManager.Health.Max);
    }

    private void OnDestroy()
    {
        if (gameManager.PlayerManager != null && gameManager.PlayerManager.Health != null)
            gameManager.PlayerManager.Health.OnChanged -= Refresh;
    }

    private void Refresh(int current, int max)
    {
        hpSlider.value = (float)current / max;
        hpText.text = current.ToString();
    }
}
