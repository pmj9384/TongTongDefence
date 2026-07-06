using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MonsterMover : MonoBehaviour
{
    public event Action<MonsterMover> OnReachedBottom;
    public event Action<MonsterMover> OnReachedPlayer;   // 돌진 도착 — 충돌 데미지 처리는 매니저 몫

    // 냉동 등 상태이상의 감속 적용 지점 (1 = 정상 속도)
    public float SpeedMultiplier { get; set; } = 1f;

    private const float ChargeArriveRadius = 0.3f;

    private Rigidbody2D rb;
    private float moveSpeed;
    private float failY;
    private bool reachedBottom;
    private bool charging;
    private Vector2 chargeTarget;
    private float chargeSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(float speed, float failY)
    {
        moveSpeed = speed;
        this.failY = failY;
        reachedBottom = false;
        charging = false;       // 풀 재사용 대비 초기화
        SpeedMultiplier = 1f;
    }

    private float chargeDelay;

    // 바닥 도달 후 delay초 머물렀다가 플레이어를 향해 돌진 (원작: 3초 뒤 회수되며 1회 데미지 — 대기 중 처치 가능)
    public void ChargeTo(Vector2 target, float speed, float delay)
    {
        chargeTarget = target;
        chargeSpeed = speed;
        chargeDelay = delay;
        charging = true;
    }

    private void FixedUpdate()
    {
        if (charging) { TickCharge(); return; }
        if (reachedBottom) return;

        Vector2 nextPosition = rb.position + Vector2.down * (moveSpeed * SpeedMultiplier) * Time.fixedDeltaTime;
        rb.MovePosition(nextPosition);

        if (nextPosition.y <= failY)
        {
            reachedBottom = true;
            OnReachedBottom?.Invoke(this);
        }
    }

    private void TickCharge()
    {
        if (chargeDelay > 0f)   // 도달 후 대기 (이 동안 볼에 맞아 죽을 수 있음)
        {
            chargeDelay -= Time.fixedDeltaTime;
            return;
        }

        Vector2 toTarget = chargeTarget - rb.position;
        if (toTarget.magnitude < ChargeArriveRadius)
        {
            charging = false;
            OnReachedPlayer?.Invoke(this);
            return;
        }
        rb.MovePosition(rb.position + toTarget.normalized * (chargeSpeed * Time.fixedDeltaTime));
    }
}
