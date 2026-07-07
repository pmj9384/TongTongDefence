using System;
using UnityEngine;

// 비매니저(Shooter 계열)가 protected GameManager에 닿는 유일한 창구 — 필요한 것만 최소 노출.
// 볼 히트 이벤트의 중계 지점이기도 하다: Ball → BallShooter → 여기 → SkillManager (매니저 간 이벤트 중계 관례)
public class BallManager : InGameManager
{
    // (ball, 맞은 몬스터 콜라이더, 충돌 노멀 — 단검 전/후면 판정용)
    public event Action<Ball, Collider2D, Vector2> OnBallHitMonster;

    public GameManager.GameState CurrentGameState => GameManager.CurrentState;
    public ObjectPoolManager ObjectPool => GameManager.ObjectPool;

    // 발사/회수 창구 — 볼 인벤토리(대기열)는 SkillManager 소관
    public bool TryGetNextLoadout(out BallLoadout loadout) => GameManager.SkillManager.TryGetNextLoadout(out loadout);
    public void ReturnBall(SkillId? skill) => GameManager.SkillManager.ReturnBall(skill);

    public void NotifyBallHitMonster(Ball ball, Collider2D monster, Vector2 hitNormal)
        => OnBallHitMonster?.Invoke(ball, monster, hitNormal);
}
