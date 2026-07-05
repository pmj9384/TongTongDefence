using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Ball : MonoBehaviour
{
    public event Action<Ball> OnHitWall;
    public event Action<Ball, Collider2D, Vector2> OnHitMonster;   // (볼, 몬스터 콜라이더, 충돌 노멀)
    public event Action<Ball> OnExitField;

    public int WallBounceCount { get; private set; }

    // 발사 시 주입되는 타입 파라미터 — Ball은 매니저를 모르고, 데미지 계산은 SkillManager 몫
    public SkillId? ActiveSkill { get; private set; }
    public int SkillLevel { get; private set; }
    public int BaseDamage { get; private set; }

    private const float ReturnArriveRadius = 0.3f;

    private Rigidbody2D rb;
    private float launchSpeed;
    private Vector2 returnTarget;   // 회수 목적지(슈터 위치) — 발사 시 값만 전달받음 (매니저 모름)
    private bool isReturning;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 direction, float speed, Vector2 returnTarget, BallLoadout loadout)
    {
        rb.gravityScale = 0f;
        rb.linearVelocity = direction.normalized * speed;
        launchSpeed = speed;
        this.returnTarget = returnTarget;
        isReturning = false;
        WallBounceCount = 0;

        ActiveSkill = loadout.skill;
        SkillLevel = loadout.level;
        BaseDamage = loadout.damage;
    }

    private void FixedUpdate()
    {
        if (!isReturning) return;

        // 바닥에 한 번 튕긴 뒤에는 슈터를 향해 유도 비행 → 도착하면 회수(풀 반환은 BallShooter 몫)
        Vector2 toTarget = returnTarget - (Vector2)transform.position;
        if (toTarget.magnitude < ReturnArriveRadius)
        {
            OnExitField?.Invoke(this);
            return;
        }
        rb.linearVelocity = toTarget.normalized * launchSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 물리 반사 후 부동소수점 드리프트를 발사 속력으로 재정규화 (모든 충돌 공통)
        rb.linearVelocity = rb.linearVelocity.normalized * launchSpeed;

        int layer = collision.gameObject.layer;
        if (layer == LayerMask.NameToLayer("Wall"))
        {
            WallBounceCount++;   // "벽" 카운트는 벽만 — 패시브 '마법 거울' 판정 근거
            OnHitWall?.Invoke(this);
            // 바닥 벽(노멀이 위)에 한 번 튕기면 회수 모드. Wall 레이어 가드 필수 —
            // 블록 윗면에 떨어져도(노멀 위) 회수되면 안 되기 때문
            if (collision.GetContact(0).normal.y > 0.5f)
                isReturning = true;
        }
        else if (layer == LayerMask.NameToLayer("Monster"))
        {
            // 원작: 볼은 블록(몬스터)에 맞으면 데미지를 주고 튕겨나감 — 소멸하지 않음.
            // 데미지 계산·적용은 이벤트를 받은 SkillManager가 담당 (Ball은 사실만 보고)
            OnHitMonster?.Invoke(this, collision.collider, collision.GetContact(0).normal);
        }
    }
}
