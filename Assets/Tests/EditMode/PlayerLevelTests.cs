using NUnit.Framework;

public class PlayerLevelTests
{
    [Test]
    public void TotalKills는_레벨업과_무관하게_누적()
    {
        var level = new PlayerLevel();
        for (int i = 0; i < 8; i++) level.AddKill();   // 3+4=7킬에서 2레벨업 후에도 누계 유지

        Assert.AreEqual(8, level.TotalKills);
        Assert.AreEqual(3, level.Level);
    }

    [Test]
    public void 첫_레벨업은_3킬()
    {
        var lv = new PlayerLevel();
        Assert.AreEqual(0, lv.AddKill());
        Assert.AreEqual(0, lv.AddKill());
        Assert.AreEqual(1, lv.AddKill());     // 3번째 킬에 레벨업
        Assert.AreEqual(2, lv.Level);
    }

    [Test]
    public void 다음_레벨업은_4킬_증가곡선()
    {
        var lv = new PlayerLevel();
        for (int i = 0; i < 3; i++) lv.AddKill();   // 2레벨 도달
        Assert.AreEqual(4, lv.KillsToNext);

        for (int i = 0; i < 3; i++) Assert.AreEqual(0, lv.AddKill());
        Assert.AreEqual(1, lv.AddKill());            // 4번째에 레벨업
        Assert.AreEqual(3, lv.Level);
        Assert.AreEqual(5, lv.KillsToNext);          // 3→4레벨은 5킬
    }

    [Test]
    public void 게이지_진행도_노출()
    {
        var lv = new PlayerLevel();
        lv.AddKill();
        Assert.AreEqual(1, lv.KillsIntoLevel);       // UI 게이지: 1/3
        Assert.AreEqual(3, lv.KillsToNext);
    }

    [Test]
    public void 레벨업_시_잔여_킬은_0부터()
    {
        var lv = new PlayerLevel();
        for (int i = 0; i < 3; i++) lv.AddKill();
        Assert.AreEqual(0, lv.KillsIntoLevel);       // 정확히 채우면 이월 없음
    }
}
