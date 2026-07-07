using System;
using TMPro;
using UnityEngine;

// 데미지 플로터 — 자기 연출(통통 튀어오르며 페이드)만 담당하는 벙어리 팝업.
// 프리팹(TMP + Kostar) 기반 — 생성/풀링/배치는 DamagePopupSpawner 몫 (표준 3층: 팝업 ← 스포너 ← 이벤트 소스)
[RequireComponent(typeof(TextMeshPro))]
public class DamagePopup : MonoBehaviour
{
    public event Action<DamagePopup> OnFinished;   // 풀 반환 신호

    private const float Lifetime = 0.7f;
    // 통통 튀는 포물선 (원작 관찰, 유저 확정 2026-07-07) — 위로 차올랐다 중력으로 살짝 떨어지며 사라짐.
    // 좌우는 결정적 분산(damage 기반) — Random 없이도 겹친 팝업이 서로 갈라져 보이게
    private const float LaunchUp = 1.7f;
    private const float LaunchSide = 0.7f;
    private const float Gravity = 5f;

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

        // 데미지 값으로 좌우 방향을 결정적으로 분산 (-1~1) — 같은 프레임 다발 팝업이 부채꼴로 퍼짐
        float side = ((damage * 7919) % 200) / 100f - 1f;
        velocity = new Vector3(side * LaunchSide, LaunchUp, 0f);
    }

    private void Update()
    {
        velocity.y -= Gravity * Time.deltaTime;   // 포물선 — 정점 찍고 살짝 낙하
        transform.position += velocity * Time.deltaTime;

        timer -= Time.deltaTime;
        text.alpha = Mathf.Clamp01(timer / Lifetime * 2f);   // 후반부에 빠르게 사라짐

        if (timer <= 0f)
            OnFinished?.Invoke(this);
    }
}
