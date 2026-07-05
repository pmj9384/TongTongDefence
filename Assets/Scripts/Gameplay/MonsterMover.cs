using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MonsterMover : MonoBehaviour
{
    public event Action<MonsterMover> OnReachedBottom;

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
    }

    private void FixedUpdate()
    {
        if (reachedBottom) return;

        Vector2 nextPosition = rb.position + Vector2.down * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(nextPosition);

        if (nextPosition.y <= failY)
        {
            reachedBottom = true;
            OnReachedBottom?.Invoke(this);
        }
    }
}
