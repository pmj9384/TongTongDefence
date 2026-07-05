// 캐릭터 레벨 — 누적 처치로 성장, 레벨업마다 3택지 발동 (기획서 "조건(처치 수) 충족 시"의 구현).
// 필요 킬 수는 증가 곡선 [가정, 원작 게이지 재관찰로 보정]: N→N+1 레벨업에 3+(N-1)킬 (3, 4, 5...)
// 순수 C# — 게이지 UI(KillsIntoLevel/KillsToNext)와 발동(AddKill 반환값)이 전부 테스트 가능.
public class PlayerLevel
{
    public const int BaseKillsToLevel = 3;   // 1→2레벨 필요 킬
    public const int GrowthPerLevel = 1;     // 레벨마다 +1킬

    public int Level { get; private set; } = 1;
    public int KillsIntoLevel { get; private set; }
    public int KillsToNext => BaseKillsToLevel + (Level - 1) * GrowthPerLevel;

    // 처치 1건 반영 → 이번 킬로 오른 레벨 수 반환 (0 또는 1 이상 — 초과분은 다음 레벨로 이월)
    public int AddKill()
    {
        KillsIntoLevel++;
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
