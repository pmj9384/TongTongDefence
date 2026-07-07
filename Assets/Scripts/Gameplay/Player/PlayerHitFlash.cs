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
    private Color[] baseColors;   // 원색 복원용 — HP바(초록/검정 유색)를 흰색으로 덮던 버그 방지
    private int lastHp = int.MinValue;
    private Coroutine flash;

    // 매니저 Initialize(GameManager.Awake) 완료 후 구독 (관례)
    private void Start()
    {
        // HP바(HpBar 하위)는 제외 — 캐릭터 몸통만 점멸. HP바까지 훑으면 유색 바가 흰색으로 덮임
        var all = GetComponentsInChildren<SpriteRenderer>(true);
        var list = new System.Collections.Generic.List<SpriteRenderer>();
        foreach (SpriteRenderer sr in all)
            if (sr.GetComponentInParent<PlayerWorldHpBar>() == null && !IsUnderHpBar(sr.transform))
                list.Add(sr);
        parts = list.ToArray();
        baseColors = new Color[parts.Length];
        for (int i = 0; i < parts.Length; i++) baseColors[i] = parts[i].color;

        context = GetComponent<PlayerContext>();
        context.Health.OnChanged += HandleHpChanged;
        lastHp = context.Health.Current;
    }

    // "HpBar"라는 이름의 조상이 있으면 체력바 파츠 (몬스터바와 동일 명명 규약)
    private static bool IsUnderHpBar(Transform t)
    {
        for (Transform p = t; p != null; p = p.parent)
            if (p.name == "HpBar") return true;
        return false;
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
            for (int k = 0; k < parts.Length; k++) parts[k].color = FlashColor;
            yield return wait;
            Restore();   // 흰색이 아니라 "원색"으로 복원
            yield return wait;
        }
        flash = null;
    }

    private void Restore()
    {
        for (int k = 0; k < parts.Length; k++) parts[k].color = baseColors[k];
    }
}
