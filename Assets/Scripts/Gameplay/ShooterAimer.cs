using System.Collections.Generic;
using UnityEngine;

public class ShooterAimer
{
    private readonly BallManager ballManager;
    private readonly LineRenderer lineRenderer;
    private readonly Transform origin;
    private readonly int maxBounces;
    private readonly float maxDistance;
    private readonly int aimMask;
    private readonly int monsterLayer;
    // Update마다 새 리스트 할당을 피하기 위해 재사용
    private readonly List<Vector3> points = new();

    public ShooterAimer(BallManager ballManager, LineRenderer lineRenderer, Transform origin, int maxBounces, float maxDistance)
    {
        this.ballManager = ballManager;
        this.lineRenderer = lineRenderer;
        this.origin = origin;
        this.maxBounces = maxBounces;
        this.maxDistance = maxDistance;
        aimMask = LayerMask.GetMask("Wall", "Monster");
        monsterLayer = LayerMask.NameToLayer("Monster");

        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 0;
    }

    public void Tick(Vector2 direction)
    {
        if (ballManager.CurrentGameState != GameManager.GameState.GamePlay) return;

        CalculateTrajectory(origin.position, direction);
        lineRenderer.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++)
            lineRenderer.SetPosition(i, points[i]);
    }

    private void CalculateTrajectory(Vector2 startPos, Vector2 dir)
    {
        points.Clear();
        points.Add(startPos);

        Vector2 pos = startPos;
        Vector2 vel = dir.normalized;
        float remaining = maxDistance;

        for (int bounce = 0; bounce < maxBounces && remaining > 0; bounce++)
        {
            var hit = Physics2D.Raycast(pos, vel, remaining, aimMask);
            if (hit.collider == null)
            {
                points.Add(pos + vel * remaining);
                break;
            }

            points.Add(hit.point);
            remaining -= hit.distance;

            if (hit.collider.gameObject.layer == monsterLayer)
                break;

            pos = hit.point;
            vel = Vector2.Reflect(vel, hit.normal);
        }
    }
}
