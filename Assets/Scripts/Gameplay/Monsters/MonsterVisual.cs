using System.Collections;
using UnityEngine;

// 타입 스프라이트 적용 + 피격 플래시 담당 — Monster(HP)/MonsterMover(이동)와 책임 분리.
// 피격은 Monster.OnDamaged 구독으로 감지 (MonsterHpBar와 동일 패턴 — Monster는 표시를 모름)
public class MonsterVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer blockRenderer;
    [SerializeField] private SpriteRenderer bodyRenderer;

    // 피격 플래시 — 기본 셰이더는 곱셈 틴트라 "흰 번쩍"이 불가능해서 빨간 틴트가 표준 대안 [눈튜닝]
    private static readonly Color FlashColor = new(1f, 0.45f, 0.45f);
    private const float FlashDuration = 0.08f;

    private Monster monster;
    private Coroutine flash;

    private void Awake()
    {
        monster = GetComponent<Monster>();
        monster.OnDamaged += HandleDamaged;
    }

    private void OnDestroy()
    {
        if (monster != null) monster.OnDamaged -= HandleDamaged;
    }

    private void OnEnable()
    {
        ResetTint();   // 풀 재사용 — 플래시 도중 회수됐어도 원색으로 시작
    }

    public void Apply(MonsterTypeData type)
    {
        blockRenderer.sprite = type.blockSprite;
        bodyRenderer.sprite = type.bodySprite;
    }

    private void HandleDamaged(Monster _, int damage, bool isCritical, SkillId? source)
    {
        if (!isActiveAndEnabled) return;
        if (flash != null) StopCoroutine(flash);
        flash = StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        blockRenderer.color = FlashColor;
        bodyRenderer.color = FlashColor;
        yield return new WaitForSeconds(FlashDuration);
        ResetTint();
        flash = null;
    }

    private void ResetTint()
    {
        blockRenderer.color = Color.white;
        bodyRenderer.color = Color.white;
    }
}
