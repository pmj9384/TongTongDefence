// 캐릭터 레벨 — 누적 처치로 성장, 레벨업마다 3택지 발동 (기획서 "조건(처치 수) 충족 시"의 구현).
// 필요 킬 수 곡선 (2026-07-06 유저 확정): 5, 10, 15, 20... — 연속 레벨업 폭주 방지 겸 난이도 곡선
// 순수 C# — 게이지 UI(KillsIntoLevel/KillsToNext)와 발동(AddKill 반환값)이 전부 테스트 가능.
public class PlayerLevel
{
    public const int BaseKillsToLevel = 5;   // 1→2레벨 필요 킬
    public const int GrowthPerLevel = 5;     // 레벨마다 +5킬 (5, 10, 15...)

    public int Level { get; private set; } = 1;
    public int KillsIntoLevel { get; private set; }
    public int TotalKills { get; private set; }   // 처치 누계 — 결과 화면 진행도(처치 비율)의 분자
    public int KillsToNext => BaseKillsToLevel + (Level - 1) * GrowthPerLevel;

    // 처치 1건 반영 → 이번 킬로 오른 레벨 수 반환 (0 또는 1 이상 — 초과분은 다음 레벨로 이월)
    public int AddKill()
    {
        KillsIntoLevel++;
        TotalKills++;
        int levelUps = 0;
        while (KillsIntoLevel >= KillsToNext)
        {
            KillsIntoLevel -= KillsToNext;
            Level++;
            levelUps++;
        }
        return levelUps;
    }
}
