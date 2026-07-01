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
    private float leftWall;
    private float rightWall;
    private float topWall;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 direction, float speed, float leftWall, float rightWall, float topWall)
    {
        rb.gravityScale = 0f;
        rb.linearVelocity = direction.normalized * speed;

        this.leftWall = leftWall;
        this.rightWall = rightWall;
        this.topWall = topWall;
        WallBounceCount = 0;
    }

    private void FixedUpdate()
    {
        float x = rb.position.x;
        float y = rb.position.y;

        if (x <= leftWall)
        {
            rb.position = new Vector2(leftWall, rb.position.y);
            rb.linearVelocity = new Vector2(Mathf.Abs(rb.linearVelocity.x), rb.linearVelocity.y);
            WallBounceCount++;
            OnHitWall?.Invoke(this);
        }
        else if (x >= rightWall)
        {
            rb.position = new Vector2(rightWall, rb.position.y);
            rb.linearVelocity = new Vector2(-Mathf.Abs(rb.linearVelocity.x), rb.linearVelocity.y);
            WallBounceCount++;
            OnHitWall?.Invoke(this);
        }

        if (y >= topWall)
        {
            rb.position = new Vector2(rb.position.x, topWall);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -Mathf.Abs(rb.linearVelocity.y));
            WallBounceCount++;
            OnHitWall?.Invoke(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
            OnHitMonster?.Invoke(this, other);
    }
}
