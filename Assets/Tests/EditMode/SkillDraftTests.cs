using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

public class SkillDraftTests
{
    private static PlayerSkills NewPlayer()
        => new PlayerSkills(SkillTableParser.Parse(File.ReadAllText("Assets/Resources/Tables/SkillTable.csv")));

    [Test]
    public void 기본_3장_중복_없음()
    {
        var p = NewPlayer();
        List<SkillId> cards = SkillDraft.Draw(p, new Random(1));
        Assert.AreEqual(3, cards.Count);
        CollectionAssert.AllItemsAreUnique(cards);
    }

    [Test]
    public void 같은_시드는_같은_결과()
    {
        var p = NewPlayer();
        CollectionAssert.AreEqual(SkillDraft.Draw(p, new Random(42)),
                                  SkillDraft.Draw(p, new Random(42)));
    }

    [Test]
    public void 액티브_만석이면_신규_액티브는_안_나옴()
    {
        var p = NewPlayer();
        p.Acquire(SkillId.FireBall);
        p.Acquire(SkillId.IceBall);
        p.Acquire(SkillId.LaserBall);
        p.Acquire(SkillId.GhostBall);   // 액티브 4 만석 — ClusterBall만 미보유

        for (int seed = 0; seed < 50; seed++)                 // 여러 시드로 규칙 위반 없음 확인
        {
            foreach (SkillId id in SkillDraft.Draw(p, new Random(seed)))
                Assert.AreNotEqual(SkillId.ClusterBall, id, $"seed {seed}: 만석인데 신규 액티브 등장");
        }
    }

    [Test]
    public void 만렙_스킬은_풀에서_제외()
    {
        var p = NewPlayer();
        p.Acquire(SkillId.FireBall); p.Acquire(SkillId.FireBall); p.Acquire(SkillId.FireBall); // Lv3

        for (int seed = 0; seed < 50; seed++)
            CollectionAssert.DoesNotContain(SkillDraft.Draw(p, new Random(seed)), SkillId.FireBall);
    }

    [Test]
    public void 후보가_전부_소진되면_노멀볼_채움_카드만()
    {
        var p = NewPlayer();
        // 액티브 4종 만렙 + 패시브 2종 만렙 → 나머지는 만석 규칙으로 전부 배제
        foreach (SkillId id in new[] { SkillId.FireBall, SkillId.IceBall, SkillId.LaserBall, SkillId.GhostBall,
                                       SkillId.TinHeart, SkillId.MagicMirror })
        {
            p.Acquire(id); p.Acquire(id); p.Acquire(id);
        }

        CollectionAssert.AreEqual(new[] { SkillId.NormalBall }, SkillDraft.Draw(p, new Random(7)));
    }

    [Test]
    public void 후보_2장이면_노멀볼이_한_자리를_채워_3장()
    {
        var p = NewPlayer();
        // 액티브 4종 만렙 + 패시브 만석(2종 보유, 그중 1종 만렙) → 후보 = 거울 업그레이드 + ...
        foreach (SkillId id in new[] { SkillId.FireBall, SkillId.IceBall, SkillId.LaserBall, SkillId.GhostBall,
                                       SkillId.TinHeart })
        {
            p.Acquire(id); p.Acquire(id); p.Acquire(id);
        }
        p.Acquire(SkillId.MagicMirror);   // 패시브 만석 — 후보는 거울 업그레이드 1장뿐

        List<SkillId> cards = SkillDraft.Draw(p, new Random(3));
        CollectionAssert.AreEqual(new[] { SkillId.MagicMirror, SkillId.NormalBall }, cards);
    }

    [Test]
    public void 후보_부족하면_있는_만큼만()
    {
        var p = NewPlayer();
        // 액티브 4종 만렙 + 패시브 1종 만렙 → 남은 후보 = 미보유 패시브 4종 (신규 가능: 패시브 1자리)
        foreach (SkillId id in new[] { SkillId.FireBall, SkillId.IceBall, SkillId.LaserBall, SkillId.GhostBall,
                                       SkillId.TinHeart })
        {
            p.Acquire(id); p.Acquire(id); p.Acquire(id);
        }

        List<SkillId> cards = SkillDraft.Draw(p, new Random(3));
        Assert.AreEqual(3, cards.Count);                      // 미보유 패시브 4종 중 3장
        foreach (SkillId id in cards)
            Assert.AreEqual(SkillKind.Passive, p.Table[id].kind);
    }
}
