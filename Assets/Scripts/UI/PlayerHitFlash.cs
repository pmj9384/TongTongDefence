using System.Collections;
using UnityEngine;

// 플레이어 피격 연출 — 캐릭터 파츠 전체를 빨갛게 점멸 (몬스터 피격 플래시와 동일 문법).
// 파츠는 자식 SpriteRenderer 자동 수집 (배선 0), 피격 감지는 PlayerHealth.OnChanged 구독.
// 매니저 접근은 PlayerContext 관문 경유 — 필요한 건 체력(Health) 계약뿐 (의존 최소화, 유저 결정 2026-07-07)
[RequireComponent(typeof(PlayerContext))]
public class PlayerHitFlash : MonoBehaviour
{
    private PlayerContext context;

    private static readonly Color FlashColor = new(1f, 0.35f, 0.35f);
    private const float FlashDuration = 0.12f;
    private const int Blinks = 2;   // 두 번 깜빡 — 한 번짜리 몬스터 피격보다 무게감 있게

    private SpriteRenderer[] parts;
    private int lastHp = int.MinValue;
    private Coroutine flash;

    // 매니저 Initialize(GameManager.Awake) 완료 후 구독 (관례)
    private void Start()
    {
        parts = GetComponentsInChildren<SpriteRenderer>(true);
        context = GetComponent<PlayerContext>();
        context.Health.OnChanged += HandleHpChanged;
        lastHp = context.Health.Current;
    }

    private void OnDestroy()
    {
        if (context != null && context.Health != null)
            context.Health.OnChanged -= HandleHpChanged;
    }

    private void HandleHpChanged(int current, int max)
    {
        bool damaged = current < lastHp;   // 회복/초기화는 연출 없음
        lastHp = current;
        if (!damaged || !isActiveAndEnabled) return;

        if (flash != null) StopCoroutine(flash);
        flash = StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        var wait = new WaitForSeconds(FlashDuration);
        for (int i = 0; i < Blinks; i++)
        {
            Tint(FlashColor);
            yield return wait;
            Tint(Color.white);
            yield return wait;
        }
        flash = null;
    }

    private void Tint(Color color)
    {
        foreach (SpriteRenderer part in parts)
            part.color = color;
    }
}
