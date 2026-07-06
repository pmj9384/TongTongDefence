using NUnit.Framework;

public class BallInventoryTests
{
    [Test]
    public void 추가한_순서대로_발사()
    {
        var inv = new BallInventory();
        inv.Add(null);                    // 노멀
        inv.Add(SkillId.FireBall);

        Assert.IsTrue(inv.TryTakeNext(out SkillId? first));
        Assert.IsNull(first);
        Assert.IsTrue(inv.TryTakeNext(out SkillId? second));
        Assert.AreEqual(SkillId.FireBall, second);
    }

    [Test]
    public void 먼저_회수된_볼부터_재발사()
    {
        var inv = new BallInventory();
        inv.Add(null);
        inv.Add(SkillId.IceBall);
        inv.TryTakeNext(out _);           // 노멀 발사
        inv.TryTakeNext(out _);           // 아이스 발사 — 대기열 빔

        inv.Return(SkillId.IceBall);      // 아이스가 먼저 회수됨
        inv.Return(null);

        inv.TryTakeNext(out SkillId? next);
        Assert.AreEqual(SkillId.IceBall, next);   // 먼저 회수된 순
    }

    [Test]
    public void 전부_필드에_나가면_발사_불가()
    {
        var inv = new BallInventory();
        inv.Add(null);
        inv.TryTakeNext(out _);

        Assert.IsFalse(inv.TryTakeNext(out _));
        Assert.AreEqual(1, inv.TotalCount);       // 보유 수는 유지
    }
}
