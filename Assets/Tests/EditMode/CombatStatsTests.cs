using NUnit.Framework;

public class CombatStatsTests
{
    [Test]
    public void 소스별로_분리_누적()
    {
        var stats = new CombatStats();
        stats.Add(null, 8);                    // 노멀
        stats.Add(SkillId.FireBall, 21);
        stats.Add(SkillId.FireBall, 8);        // 화상 틱도 파이어 귀속
        stats.Add(null, 8);

        Assert.AreEqual(16, stats.TotalOf(null));
        Assert.AreEqual(29, stats.TotalOf(SkillId.FireBall));
        Assert.AreEqual(0, stats.TotalOf(SkillId.IceBall));   // 미기록 소스는 0
    }

    [Test]
    public void 최대_누적은_비율_바의_분모()
    {
        var stats = new CombatStats();
        stats.Add(null, 100);
        stats.Add(SkillId.LaserBall, 40);

        Assert.AreEqual(100, stats.MaxTotal);
    }

    [Test]
    public void DPS_계산과_0초_가드()
    {
        Assert.AreEqual(31, CombatStats.Dps(2209, 70f));   // 원작 스샷 수치 감각
        Assert.AreEqual(0, CombatStats.Dps(100, 0f));      // 시작 직후 0 나눗셈 방지
    }
}
