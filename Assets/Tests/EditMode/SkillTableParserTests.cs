using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

// 실제 CSV(Resources/Tables/SkillTable.csv)를 그대로 검증 — 수치 오타를 테스트가 잡는다.
// EditMode 테스트는 프로젝트 루트가 CWD라 파일 직접 읽기 가능.
public class SkillTableParserTests
{
    private const string CsvPath = "Assets/Resources/Tables/SkillTable.csv";

    private static Dictionary<SkillId, SkillDef> LoadReal()
        => SkillTableParser.Parse(File.ReadAllText(CsvPath));

    [Test]
    public void 실제_CSV_10종_3레벨_전부_로드()
    {
        var table = LoadReal();
        Assert.AreEqual(10, table.Count);
        foreach (SkillDef def in table.Values)
        {
            Assert.AreEqual(3, def.levels.Length, $"{def.id} 레벨 수");
            Assert.IsNotEmpty(def.displayName, $"{def.id} 이름");
        }
    }

    [Test]
    public void 수치_스팟체크_기획서_원문과_일치()
    {
        var table = LoadReal();

        SkillLevel fire1 = table[SkillId.FireBall].GetLevel(1);
        Assert.AreEqual(21, fire1.ballDamage);
        Assert.AreEqual(4f, fire1.a);      // 화상 지속
        Assert.AreEqual(3f, fire1.b);      // 최대 중첩
        Assert.AreEqual(8f, fire1.c);      // 중첩당 초당 피해

        SkillLevel ice3 = table[SkillId.IceBall].GetLevel(3);
        Assert.AreEqual(50, ice3.ballDamage);
        Assert.AreEqual(0.4f, ice3.a);     // 냉동 확률
        Assert.AreEqual(7f, ice3.b);       // 지속
        Assert.AreEqual(0.2f, ice3.c);     // 감속=받피증

        Assert.AreEqual(0.4f, table[SkillId.MagicMirror].GetLevel(2).a);
        Assert.AreEqual(15f, table[SkillId.LaserBall].GetLevel(3).a);
        Assert.AreEqual(30f, table[SkillId.LastMatch].GetLevel(3).a);
        Assert.AreEqual(0, table[SkillId.TinHeart].GetLevel(1).ballDamage); // 패시브는 볼데미지 0
    }

    [Test]
    public void 종류_구분_액티브5_패시브5()
    {
        var table = LoadReal();
        int active = 0, passive = 0;
        foreach (SkillDef def in table.Values)
            if (def.kind == SkillKind.ActiveBall) active++; else passive++;
        Assert.AreEqual(5, active);
        Assert.AreEqual(5, passive);
    }

    [Test]
    public void 컬럼_부족한_행은_예외()
    {
        Assert.Throws<System.FormatException>(() =>
            SkillTableParser.Parse("header\nFireBall,ActiveBall,1,21\n"));
    }

    [Test]
    public void 레벨_범위_밖은_예외()
    {
        Assert.Throws<System.FormatException>(() =>
            SkillTableParser.Parse("header\nFireBall,ActiveBall,4,21,4,3,8,파이어 볼\n"));
    }
}
