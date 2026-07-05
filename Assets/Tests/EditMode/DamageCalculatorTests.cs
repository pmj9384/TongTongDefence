using System;
using NUnit.Framework;

public class DamageCalculatorTests
{
    // 기본 컨텍스트: 노멀볼 8, 보정 전부 0 — 각 테스트가 필요한 항목만 켠다
    private static DamageCalculator.Context Base() => new DamageCalculator.Context
    {
        baseDamage = 8,
        isNormalBall = true,
        critMultiplier = 1.5f,
    };

    [Test]
    public void 보정_없으면_기본값_그대로()
    {
        int dmg = DamageCalculator.Calc(Base(), new Random(1), out bool crit);
        Assert.AreEqual(8, dmg);          // 기획서 기본값: 노멀 8, 치명타 0%
        Assert.IsFalse(crit);
    }

    [Test]
    public void 양철심장은_노멀볼에만()
    {
        var ctx = Base();
        ctx.tinHeartBonus = 0.2f;                       // Lv1
        Assert.AreEqual(10, Calc(ctx));                 // 8 × 1.2 = 9.6 → 10

        ctx.isNormalBall = false;                       // 스킬볼이면 미적용
        Assert.AreEqual(8, Calc(ctx));
    }

    [Test]
    public void 마법거울은_벽_반사_횟수만큼_누적()
    {
        var ctx = Base();
        ctx.mirrorPerBounce = 0.2f;                     // Lv1

        ctx.wallBounces = 0;
        Assert.AreEqual(8, Calc(ctx));                  // 안 튕겼으면 보정 없음

        ctx.wallBounces = 3;
        Assert.AreEqual(13, Calc(ctx));                 // 8 × (1 + 0.2×3) = 12.8 → 13
    }

    [Test]
    public void 냉동_대상은_받는_피해_증가_그리고_0점5는_올림()
    {
        var ctx = Base();
        ctx.baseDamage = 25;                            // 아이스볼 Lv1
        ctx.isNormalBall = false;
        ctx.targetFrozen = true;
        ctx.frozenBonus = 0.1f;
        Assert.AreEqual(28, Calc(ctx));                 // 25 × 1.1 = 27.5 → 28 (은행가 반올림이면 27이 됨 — 방식 고정 검증)

        ctx.targetFrozen = false;
        Assert.AreEqual(25, Calc(ctx));
    }

    [Test]
    public void 치명타_확률_경계_0과_100()
    {
        var ctx = Base();

        ctx.critChance = 0f;
        DamageCalculator.Calc(ctx, new Random(1), out bool crit0);
        Assert.IsFalse(crit0);

        ctx.critChance = 1f;
        int dmg = DamageCalculator.Calc(ctx, new Random(1), out bool crit100);
        Assert.IsTrue(crit100);
        Assert.AreEqual(12, dmg);                       // 8 × 1.5 (치명타 데미지율 50%)
    }

    [Test]
    public void 전체_조합_적용_순서_고정()
    {
        var ctx = Base();
        ctx.tinHeartBonus = 0.2f;
        ctx.mirrorPerBounce = 0.2f;
        ctx.wallBounces = 2;
        ctx.targetFrozen = true;
        ctx.frozenBonus = 0.1f;
        ctx.critChance = 1f;
        // 8 × 1.2 × (1+0.4) × 1.1 × 1.5 = 22.176 → 22
        int dmg = DamageCalculator.Calc(ctx, new Random(1), out _);
        Assert.AreEqual(22, dmg);
    }

    private static int Calc(DamageCalculator.Context ctx)
        => DamageCalculator.Calc(ctx, new Random(1), out _);
}
