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

    private Rigidbody2D rb;
    private float launchSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 direction, float speed)
    {
        rb.gravityScale = 0f;
        rb.linearVelocity = direction.normalized * speed;
        launchSpeed = speed;
        WallBounceCount = 0;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        WallBounceCount++;
        // 물리 반사 후 부동소수점 오차로 속도가 드리프트되는 것을 발사 속력으로 재정규화해 보정
        rb.linearVelocity = rb.linearVelocity.normalized * launchSpeed;
        OnHitWall?.Invoke(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            OnHitMonster?.Invoke(this, other);
            other.GetComponent<Monster>().TakeDamage(8, false);   // TEMP: SkillManager 플랜에서 이벤트 기반으로 교체
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            // Left/Right/Top은 non-trigger 물리 벽이라 여기 안 걸림 — 트리거인 BottomWall만 해당
            OnExitField?.Invoke(this);
        }
    }
}
