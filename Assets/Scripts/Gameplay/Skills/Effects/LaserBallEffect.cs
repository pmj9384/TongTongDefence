using System.Collections.Generic;

// 레이저볼: 타격한 몬스터와 "같은 행"의 모든 적에게 피해 — a=행 피해.
// 행 판정 = 격자 API 기반 Y밴드 쿼리 (몬스터는 연속 하강이라 행 인덱스보다 Y 근사가 정확).
// 부가 피해라 치명타/패시브 증폭 미적용 [가정5]
public class LaserBallEffect : IOnHitEffect
{
    private readonly MonsterManager monsterManager;

    public LaserBallEffect(MonsterManager monsterManager) => this.monsterManager = monsterManager;

    public void Apply(Monster target, SkillLevel data)
    {
        foreach (Monster m in monsterManager.GetMonstersNearRow(target.transform.position.y))
            m.TakeDamage((int)data.a, false, SkillId.LaserBall);
    }
}
