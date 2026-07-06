using UnityEngine;
using TMPro;
using UnityEngine.UI;

// 플레이어 HP 게이지 — 값 갱신만. Slider/텍스트는 BottomBar 존에 에디터 조립 (표준 UGUI Slider, 핸들 없는 게이지).
// 색·크기·위치는 전부 Inspector 소관.
public class PlayerHpBar : MonoBehaviour
{
    [SerializeField] private PlayerManager playerManager;

    [Header("씬 참조 (BottomBar 아래)")]
    [SerializeField] private Slider hpSlider;   // interactable 꺼진 게이지
    [SerializeField] private TMP_Text hpText;

    private void Start()   // 매니저 Initialize 완료 후 (관례)
    {
        playerManager.Health.OnChanged += Refresh;
        Refresh(playerManager.Health.Current, playerManager.Health.Max);
    }

    private void OnDestroy()
    {
        if (playerManager != null && playerManager.Health != null)
            playerManager.Health.OnChanged -= Refresh;
    }

    private void Refresh(int current, int max)
    {
        hpSlider.value = (float)current / max;
        hpText.text = current.ToString();
    }
}
