using System.Collections.Generic;

// 전투 집계 — 소스(볼/스킬)별 누적 데미지만 담당 (순수 C#, 테스트 대상).
// 시간/DPS의 "경과" 값은 호출측(StatsManager)이 주입 — 코어는 엔진 시계를 모른다.
// 원작 전투 정보 창 관찰(#57): 노멀 볼 포함 소스별 [누적 / DPS / 최대 대비 비율] 표시.
public class CombatStats
{
    // Dictionary는 null 키 금지 — 노멀볼(source=null)은 내부 -1 키로 변환 (Play 실버그에서 확인)
    private readonly Dictionary<int, long> totals = new();

    private static int Key(SkillId? source) => source.HasValue ? (int)source.Value : -1;

    public void Add(SkillId? source, int damage)
    {
        int key = Key(source);
        totals.TryGetValue(key, out long current);
        totals[key] = current + damage;
    }

    public long TotalOf(SkillId? source)
        => totals.TryGetValue(Key(source), out long total) ? total : 0;

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
