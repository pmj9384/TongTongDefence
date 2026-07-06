using System;
using System.Collections.Generic;
using System.Globalization;

// CSV 텍스트 → 스킬 정의 사전. 순수 함수 — 파일/Resources 접근은 호출측(SkillManager) 몫.
// 스키마: skillId,kind,level,ballDamage,a,b,c,displayName,description,icon (헤더 1행 + 30행)
// CsvHelper 미사용 — 값에 쉼표/따옴표가 없는 통제된 스키마라 미니 파서로 충분 (AnimalBreakOut 패턴에서 의존성만 제거)
public static class SkillTableParser
{
    private const int LevelCount = 3;

    public static Dictionary<SkillId, SkillDef> Parse(string csvText)
    {
        var table = new Dictionary<SkillId, SkillDef>();
        string[] lines = csvText.Split('\n');

        for (int i = 1; i < lines.Length; i++)   // 0행 = 헤더
        {
            string line = lines[i].Trim();
            if (line.Length == 0) continue;

            string[] cols = line.Split(',');
            if (cols.Length < 10)
                throw new FormatException($"SkillTable {i + 1}행: 컬럼 수 부족 ({cols.Length})");

            var id = (SkillId)Enum.Parse(typeof(SkillId), cols[0]);
            var kind = (SkillKind)Enum.Parse(typeof(SkillKind), cols[1]);
            int level = int.Parse(cols[2], CultureInfo.InvariantCulture);
            if (level < 1 || level > LevelCount)
                throw new FormatException($"SkillTable {i + 1}행: 레벨 범위 오류 ({level})");

            if (!table.TryGetValue(id, out SkillDef def))
            {
                def = new SkillDef
                {
                    id = id, kind = kind, displayName = cols[7],
                    description = cols[8], iconName = cols[9],
                    levels = new SkillLevel[LevelCount],
                };
                table.Add(id, def);
            }

            def.levels[level - 1] = new SkillLevel
            {
                ballDamage = int.Parse(cols[3], CultureInfo.InvariantCulture),
                a = float.Parse(cols[4], CultureInfo.InvariantCulture),
                b = float.Parse(cols[5], CultureInfo.InvariantCulture),
                c = float.Parse(cols[6], CultureInfo.InvariantCulture),
            };
        }

        return table;
    }
}
