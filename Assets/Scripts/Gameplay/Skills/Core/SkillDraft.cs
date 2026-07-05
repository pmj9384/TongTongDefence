using System;
using System.Collections.Generic;

// 3택지 추출 — 기획서 규칙 (프로젝트에서 유일하게 Random이 허용된 곳: PDF가 "랜덤 추출" 명시):
//   미보유 스킬 → Lv1 카드만 / 종류(액티브4·패시브2) 만석 → 그 종류는 보유 스킬 업그레이드만
//   레벨 1~3 (만렙은 풀에서 제외) / 전체 풀에서 랜덤 / 동일 카드 중복 없음
public static class SkillDraft
{
    public const int CardCount = 3;

    public static List<SkillId> Draw(PlayerSkills owned, Random rng)
    {
        var candidates = new List<SkillId>();
        foreach (SkillDef def in owned.Table.Values)
        {
            int lv = owned.GetLevel(def.id);
            if (lv >= PlayerSkills.MaxLevel) continue;               // 만렙 제외
            if (lv == 0 && owned.IsFull(def.kind)) continue;         // 만석 종류는 신규 금지
            candidates.Add(def.id);                                  // lv==0 → Lv1 카드, lv>0 → 업그레이드 카드
        }

        // 비복원 무작위 추출 (후보가 3장 미만이면 있는 만큼)
        var result = new List<SkillId>();
        int picks = Math.Min(CardCount, candidates.Count);
        for (int i = 0; i < picks; i++)
        {
            int index = rng.Next(candidates.Count);
            result.Add(candidates[index]);
            candidates.RemoveAt(index);
        }
        return result;
    }
}
