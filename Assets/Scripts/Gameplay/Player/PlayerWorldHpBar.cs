using TMPro;
using UnityEngine;

// 플레이어 체력바 — 캐릭터 발밑 "월드" 바 (원작 #84 캡슐). 몬스터 HpBar와 동일 문법.
// 캔버스 배치는 캐릭터(월드)와 좌표계가 달라 화면비/노치마다 어긋나서 폐기 (실기기 확인 2026-07-07).
// 구조·배치·색은 씬 소관 (Build Player HpBar 메뉴로 조립), 여기는 값 갱신만.
public class PlayerWorldHpBar : MonoBehaviour
{
    [Header("씬 배선 (빌더 조립: HpBar > Background/Fill/Value)")]
    [SerializeField] private Transform fill;
    [SerializeField] private TMP_Text valueText;

    private PlayerContext context;
    private float baseWidth;
    private float baseX;

    private void Start()   // 매니저 Initialize 완료 후 (관례)
    {
        context = GetComponent<PlayerContext>();
        baseWidth = fill.localScale.x;
        baseX = fill.localPosition.x;
        context.Health.OnChanged += Refresh;
        Refresh(context.Health.Current, context.Health.Max);
    }

    private void OnDestroy()
    {
        if (context != null && context.Health != null)
            context.Health.OnChanged -= Refresh;
    }

    private void Refresh(int current, int max)
    {
        float ratio = Mathf.Clamp01((float)current / max);
        Vector3 scale = fill.localScale;
        scale.x = baseWidth * ratio;
        fill.localScale = scale;

        Vector3 pos = fill.localPosition;
        pos.x = baseX - baseWidth * (1f - ratio) * 0.5f;   // 왼쪽 기준 채움
        fill.localPosition = pos;

        valueText.text = current.ToString();
    }
}
