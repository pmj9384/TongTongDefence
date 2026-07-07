using System;
using UnityEngine;

// 클러스터볼: 타격 시 확률로 파편볼 생성 — a=확률, b=파편 피해.
// 파편의 실제 스폰/풀링은 주입받은 델리게이트(SkillManager 소유)가 담당 — 효과는 "언제, 얼마"만 결정
public class ClusterBallEffect : IOnHitEffect
{
    private readonly System.Random rng;
    private readonly Action<Vector2, int> spawnFragment;   // (스폰 위치, 파편 피해)

    public ClusterBallEffect(System.Random rng, Action<Vector2, int> spawnFragment)
    {
        this.rng = rng;
        this.spawnFragment = spawnFragment;
    }

    public void Apply(Monster target, SkillLevel data)
    {
        if (rng.NextDouble() < data.a)
            spawnFragment(target.transform.position, (int)data.b);
    }
}
