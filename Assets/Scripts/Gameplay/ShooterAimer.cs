using System.Collections.Generic;
using UnityEngine;

public class ShooterAimer
{
    private readonly BallManager ballManager;
    private readonly LineRenderer lineRenderer;
    private readonly Transform origin;
    private readonly int maxBounces;
    private readonly float maxDistance;
    // 매 프레임 새 리스트 할당을 피하기 위해 재사용
    private readonly List<Vector3> points = new();

    public ShooterAimer(BallManager ballManager, LineRenderer lineRenderer, Transform origin, int maxBounces, float maxDistance)
    {
        this.ballManager = ballManager;
        this.lineRenderer = lineRenderer;
        this.origin = origin;
        this.maxBounces = maxBounces;
        this.maxDistance = maxDistance;

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

        FieldManager field = ballManager.Field;
        Vector2 pos = startPos;
        Vector2 vel = dir.normalized;
        float remaining = maxDistance;

        for (int bounce = 0; bounce < maxBounces && remaining > 0; bounce++)
        {
            float distToLeft = vel.x < 0 ? (field.LeftWall - pos.x) / vel.x : float.MaxValue;
            float distToRight = vel.x > 0 ? (field.RightWall - pos.x) / vel.x : float.MaxValue;
            float distToTop = vel.y > 0 ? (field.TopWall - pos.y) / vel.y : float.MaxValue;

            var hit = Physics2D.Raycast(pos, vel, remaining, LayerMask.GetMask("Monster"));
            float distToMonster = hit.collider != null ? hit.distance : float.MaxValue;

            float minDist = Mathf.Min(distToLeft, distToRight, distToTop, distToMonster, remaining);
            pos += vel * minDist;
            points.Add(pos);
            remaining -= minDist;

            if (minDist == distToMonster) break;
            if (minDist == distToLeft || minDist == distToRight) vel.x = -vel.x;
            if (minDist == distToTop) vel.y = -vel.y;
        }
    }
}
