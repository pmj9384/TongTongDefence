using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Ball : MonoBehaviour
{
    public event Action<Ball> OnHitWall;
    public event Action<Ball, Collider2D> OnHitMonster;
    public event Action<Ball> OnExitField;

    public int WallBounceCount { get; private set; }

    private const float ReturnArriveRadius = 0.3f;

    private Rigidbody2D rb;
    private float launchSpeed;
    private Vector2 returnTarget;   // 회수 목적지(슈터 위치) — 발사 시 값만 전달받음 (매니저 모름)
    private bool isReturning;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 direction, float speed, Vector2 returnTarget)
    {
        rb.gravityScale = 0f;
        rb.linearVelocity = direction.normalized * speed;
        launchSpeed = speed;
        this.returnTarget = returnTarget;
        isReturning = false;
        WallBounceCount = 0;
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
        WallBounceCount++;
        // 물리 반사 후 부동소수점 오차로 속도가 드리프트되는 것을 발사 속력으로 재정규화해 보정
        rb.linearVelocity = rb.linearVelocity.normalized * launchSpeed;
        OnHitWall?.Invoke(this);

        // 바닥 벽만 노멀이 위(+y)를 향한다 (좌/우 = ±x, 천장 = -y) — 바닥에서 한 번 튕기면 회수 모드.
        // 회수 모드 전에는 슈터와 겹쳐도 아무 일 없음 (도착 판정은 isReturning일 때만)
        if (collision.GetContact(0).normal.y > 0.5f)
            isReturning = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            OnHitMonster?.Invoke(this, other);
            other.GetComponent<Monster>().TakeDamage(8, false);   // TEMP: SkillManager 플랜에서 이벤트 기반으로 교체
        }
    }
}
