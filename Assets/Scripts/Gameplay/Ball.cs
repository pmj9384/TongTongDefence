using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Ball : MonoBehaviour
{
    public event Action<Ball> OnHitWall;
    public event Action<Ball, Collider2D> OnHitMonster;

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
            OnHitMonster?.Invoke(this, other);
    }
}
