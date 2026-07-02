using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ShooterAimer : MonoBehaviour
{
    [SerializeField] private Transform shootOrigin;
    [SerializeField] private int maxBounces = 2;
    [SerializeField] private float maxDistance = 30f;

    private BallManager ballManager;
    private ShooterInputHandler inputHandler;
    private LineRenderer lineRenderer;
    private Vector2 currentDirection = Vector2.up;

    private void Awake()
    {
        ballManager = GetComponent<BallManager>();
        inputHandler = GetComponent<ShooterInputHandler>();
        inputHandler.OnDirectionChanged += dir => currentDirection = dir;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 0;
    }

    private void Update()
    {
        if (ballManager.CurrentGameState != GameManager.GameState.GamePlay) return;

        var points = CalculateTrajectory(shootOrigin.position, currentDirection);
        lineRenderer.positionCount = points.Length;
        for (int i = 0; i < points.Length; i++)
            lineRenderer.SetPosition(i, points[i]);
    }

    private Vector3[] CalculateTrajectory(Vector2 origin, Vector2 dir)
    {
        var points = new List<Vector3> { origin };
        Vector2 pos = origin;
        Vector2 vel = dir.normalized;
        float remaining = maxDistance;

        for (int bounce = 0; bounce < maxBounces && remaining > 0; bounce++)
        {
            float distToLeft = vel.x < 0 ? (ballManager.LeftWall - pos.x) / vel.x : float.MaxValue;
            float distToRight = vel.x > 0 ? (ballManager.RightWall - pos.x) / vel.x : float.MaxValue;
            float distToTop = vel.y > 0 ? (ballManager.TopWall - pos.y) / vel.y : float.MaxValue;

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

        return points.ToArray();
    }
}
