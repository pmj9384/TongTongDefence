using UnityEngine;

// 인게임 수치 집계의 단일 창구 (AnimalBreakOut InGameCountManager 패턴) —
// 이벤트로 흘러오는 카운트류는 전부 여기로 모은다. 계산 규칙은 순수 코어(CombatStats) 몫.
// UI(전투 정보 창)는 GameManager.StatsManager 한 곳만 본다.
public class StatsManager : InGameManager
{
    public CombatStats Combat { get; private set; }

    // 전투 경과(초) — Time.time은 timeScale을 타므로 정지(선택/퍼즈/결과) 시간이 자동 제외된다 [가정, 원작 DPS 감각]
    public float CombatElapsed => Time.time - combatStartTime;

    private float combatStartTime;

    public override void Initialize()
    {
        base.Initialize();
        Combat = new CombatStats();
        combatStartTime = Time.time;

        GameManager.MonsterManager.OnDamageDealt += HandleDamageDealt;
    }

    public override void Clear()
    {
        base.Clear();
        GameManager.MonsterManager.OnDamageDealt -= HandleDamageDealt;
    }

    private void HandleDamageDealt(SkillId? source, int damage) => Combat.Add(source, damage);
}
