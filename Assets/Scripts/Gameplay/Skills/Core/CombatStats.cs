using System.Collections.Generic;

// 전투 집계 — 소스(볼/스킬)별 누적 데미지만 담당 (순수 C#, 테스트 대상).
// 시간/DPS의 "경과" 값은 호출측(StatsManager)이 주입 — 코어는 엔진 시계를 모른다.
// 원작 전투 정보 창 관찰(#57): 노멀 볼 포함 소스별 [누적 / DPS / 최대 대비 비율] 표시.
public class CombatStats
{
    private readonly Dictionary<SkillId?, long> totals = new();   // null 키 = 노멀볼

    public IReadOnlyDictionary<SkillId?, long> Totals => totals;

    public void Add(SkillId? source, int damage)
    {
        totals.TryGetValue(source, out long current);
        totals[source] = current + damage;
    }

    public long TotalOf(SkillId? source)
        => totals.TryGetValue(source, out long total) ? total : 0;

    // 비율 바의 분모 — 가장 많이 넣은 소스의 누적
    public long MaxTotal
    {
        get
        {
            long max = 0;
            foreach (long total in totals.Values)
                if (total > max) max = total;
            return max;
        }
    }

    public static int Dps(long total, float elapsedSeconds)
        => elapsedSeconds <= 0f ? 0 : (int)(total / elapsedSeconds);
}
