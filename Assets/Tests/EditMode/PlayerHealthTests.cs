using NUnit.Framework;

public class PlayerHealthTests
{
    [Test]
    public void 데미지_누적과_사망()
    {
        var hp = new PlayerHealth(300);
        bool died = false;
        hp.OnDied += () => died = true;

        hp.TakeDamage(100);
        Assert.AreEqual(200, hp.Current);
        Assert.IsFalse(died);

        hp.TakeDamage(250);                  // 과잉 데미지 — 0 밑으로 안 내려감
        Assert.AreEqual(0, hp.Current);
        Assert.IsTrue(died);
    }

    [Test]
    public void 사망_후_추가_데미지_무시_그리고_OnDied_1회만()
    {
        var hp = new PlayerHealth(10);
        int diedCount = 0;
        hp.OnDied += () => diedCount++;

        hp.TakeDamage(10);
        hp.TakeDamage(10);
        Assert.AreEqual(1, diedCount);
        Assert.AreEqual(0, hp.Current);
    }

    [Test]
    public void OnChanged는_현재와_최대를_전달()
    {
        var hp = new PlayerHealth(300);
        int cur = -1, max = -1;
        hp.OnChanged += (c, m) => { cur = c; max = m; };

        hp.TakeDamage(30);
        Assert.AreEqual(270, cur);
        Assert.AreEqual(300, max);
    }
}
