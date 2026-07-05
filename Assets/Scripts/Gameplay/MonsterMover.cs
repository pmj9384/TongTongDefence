using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MonsterMover : MonoBehaviour
{
    public event Action<MonsterMover> OnReachedBottom;

    // 냉동 등 상태이상의 감속 적용 지점 (1 = 정상 속도)
    public float SpeedMultiplier { get; set; } = 1f;

    private Rigidbody2D rb;
    private float moveSpeed;
    private float failY;
    private bool reachedBottom;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(float speed, float failY)
    {
        moveSpeed = speed;
        this.failY = failY;
        reachedBottom = false;
        SpeedMultiplier = 1f;   // 풀 재사용 대비 초기화
    }

    private void FixedUpdate()
    {
        if (reachedBottom) return;

        Vector2 nextPosition = rb.position + Vector2.down * (moveSpeed * SpeedMultiplier) * Time.fixedDeltaTime;
        rb.MovePosition(nextPosition);

        if (nextPosition.y <= failY)
        {
            reachedBottom = true;
            OnReachedBottom?.Invoke(this);
        }
    }
}
