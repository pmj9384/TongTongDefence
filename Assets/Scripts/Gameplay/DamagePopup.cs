using System;
using TMPro;
using UnityEngine;

// 데미지 플로터 — 자기 연출(통통 튀어오르며 페이드)만 담당하는 벙어리 팝업.
// 프리팹(TMP + Kostar) 기반 — 생성/풀링/배치는 DamagePopupSpawner 몫 (표준 3층: 팝업 ← 스포너 ← 이벤트 소스)
[RequireComponent(typeof(TextMeshPro))]
public class DamagePopup : MonoBehaviour
{
    public event Action<DamagePopup> OnFinished;   // 풀 반환 신호

    private const float Lifetime = 0.5f;
    // 원작(#82) 재확인: 몬스터 중앙 근처에서 "살짝만" 떠오르고 사라짐 — 포물선/하강 없음 (유저 정정 2026-07-07).
    // 좌우 미세 분산(damage 기반 결정적)만 유지 — 겹친 팝업이 완전히 포개지지 않게
    private const float RiseSpeed = 0.15f;
    private const float SideSpread = 0.12f;

    private TMP_Text text;
    private float timer;
    private Vector3 velocity;

    private void Awake() => text = GetComponent<TMP_Text>();

    public void Show(Vector3 position, int damage, bool isCritical)
    {
        transform.position = position;
        timer = Lifetime;

        text.text = damage.ToString();
        text.color = isCritical ? new Color(1f, 0.3f, 0.25f) : Color.white;
        transform.localScale = Vector3.one * (isCritical ? 1.4f : 1f);   // 치명타 강조

        // 데미지 값으로 좌우 방향을 결정적으로 미세 분산 (-1~1)
        float side = ((damage * 7919) % 200) / 100f - 1f;
        velocity = new Vector3(side * SideSpread, RiseSpeed, 0f);
    }

    private void Update()
    {
        transform.position += velocity * Time.deltaTime;   // 살짝 상승만 (원작 #82)

        timer -= Time.deltaTime;
        text.alpha = Mathf.Clamp01(timer / Lifetime * 2f);   // 후반부에 빠르게 사라짐

        if (timer <= 0f)
            OnFinished?.Invoke(this);
    }
}
