using UnityEngine;

// 마지막 성냥: 적 사망 시 폭발 — 근처 적에게 피해 (a=폭발 피해).
// 온히트가 아니라 사망 트리거라 IOnHitEffect가 아닌 별도 진입점.
// 반경 = 셀 1.5칸 [가정], 부가 피해라 치명타/증폭 미적용 [가정5]
public class LastMatchEffect
{
    private readonly FieldManager fieldManager;
    private readonly int monsterLayerMask;

    public LastMatchEffect(FieldManager fieldManager)
    {
        this.fieldManager = fieldManager;
        monsterLayerMask = LayerMask.GetMask("Monster");
    }

    public void Explode(Vector2 center, SkillLevel data)
    {
        float radius = fieldManager.CellWidth * 1.5f;
        foreach (Collider2D hit in Physics2D.OverlapCircleAll(center, radius, monsterLayerMask))
            hit.GetComponent<Monster>()?.TakeDamage((int)data.a, false);
    }
}
