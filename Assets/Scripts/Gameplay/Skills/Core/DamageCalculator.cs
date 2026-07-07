using System;

// 데미지 파이프라인 — 프로젝트의 모든 "볼 → 몬스터" 데미지는 이 함수 하나를 지난다.
// 순수 static 함수: 상태 없음, Random 주입 → 조합 전부 EditMode 테스트 가능.
// (스킬 10종×3레벨 조합은 수동 Play로 회귀 검증이 불가능한 영역이라 테스트가 필수)
//
// 공식(적용 순서 고정):
//   최종 = 반올림( 기본데미지
//          × (1 + 양철심장%)            — 노멀볼일 때만
//          × (1 + 마법거울% × 벽바운스) — 이번 발사에서 벽에 튕긴 횟수(Ball.WallBounceCount)
//          × (1 + 냉동 받피증%)         — 대상이 냉동 상태일 때만
//          × (치명타면 1.5) )           — 확률은 단검 패시브 합산(기본 0%)
public static class DamageCalculator
{
    public struct Context
    {
        public int baseDamage;         // 볼 타입·레벨 데미지 (노멀볼 = 8)
        public bool isNormalBall;
        public float tinHeartBonus;    // 양철심장 a값 (미보유 0)
        public float mirrorPerBounce;  // 마법거울 a값 (미보유 0)
        public int wallBounces;        // 이번 발사의 벽 반사 횟수
        public bool targetFrozen;
        public float frozenBonus;      // 아이스볼 c값 (냉동 아닐 때 무시)
        public float critChance;       // 0~1. 단검 합산 결과 ("적당 1회" 소모는 호출측 책임)
        public float critMultiplier;   // 기본 1.5 (치명타 데미지율 50%)
    }

    public static int Calc(Context c, Random rng, out bool isCrit)
    {
        float damage = c.baseDamage;

        if (c.isNormalBall)
            damage *= 1f + c.tinHeartBonus;

        damage *= 1f + c.mirrorPerBounce * c.wallBounces;

        if (c.targetFrozen)
            damage *= 1f + c.frozenBonus;

        isCrit = rng.NextDouble() < c.critChance;
        if (isCrit)
            damage *= c.critMultiplier;

        // C# 기본 Math.Round는 은행가 반올림(0.5→짝수)이라 26.5→26이 됨.
        // 데미지는 "0.5는 올림"이 직관적이므로 AwayFromZero로 고정
        return (int)Math.Round(damage, MidpointRounding.AwayFromZero);
    }
}
