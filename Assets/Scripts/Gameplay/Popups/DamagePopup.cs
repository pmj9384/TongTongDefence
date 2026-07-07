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
    private static readonly Vector2 SpawnJitter = new(0.14f, 0.09f);   // 중앙 "주변" 좁은 랜덤 (원작 #82 — 연출용이라 Random 허용)

    private TMP_Text text;
    private float timer;
    private Vector3 velocity;

    private void Awake() => text = GetComponent<TMP_Text>();

    public void Show(Vector3 position, int damage, bool isCritical)
    {
        // 정중앙 고정 대신 좁은 지터 — 연타 시 숫자가 완전히 포개지지 않으면서도 몬스터를 벗어나지 않게
        transform.position = position + new Vector3(
            UnityEngine.Random.Range(-SpawnJitter.x, SpawnJitter.x), UnityEngine.Random.Range(-SpawnJitter.y, SpawnJitter.y), 0f);
        timer = Lifetime;

        text.text = damage.ToString();
        text.color = isCritical ? new Color(1f, 0.3f, 0.25f) : Color.white;
        transform.localScale = Vector3.one * (isCritical ? 1.4f : 1f);   // 치명타 강조

        velocity = new Vector3(0f, RiseSpeed, 0f);   // 이동은 위로 살짝만 — 분산은 스폰 지터가 담당
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
