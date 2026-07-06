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
        if (ballManager.CurrentGameState != GameManager.GameState.GamePlay)
        {
            lineRenderer.positionCount = 0;   // 정지/선택/결과 화면에 마지막 조준선이 박제되지 않게
            return;
        }

        CalculateTrajectory(origin.position, direction);
        lineRenderer.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++)
            lineRenderer.SetPosition(i, points[i]);
    }

    // 조준선이 캐릭터 스프라이트를 뚫고 시작하지 않게 — 원작처럼 캐릭터 바깥에서 시작
    private const float LineStartOffset = 0.45f;

    private void CalculateTrajectory(Vector2 startPos, Vector2 dir)
    {
        points.Clear();
        Vector2 vel = dir.normalized;
        startPos += vel * LineStartOffset;
        points.Add(startPos);

        Vector2 pos = startPos;
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

            vel = Vector2.Reflect(vel, hit.normal);
            // 표면 정확히 그 점에서 재출발하면 부동소수점에 따라 같은 벽을 거리 0으로 재히트
            // → 반사 횟수만 소진되고 선이 끊김 (측면 벽에서 간헐 재현). 진행 방향으로 살짝 밀어 탈출
            pos = hit.point + vel * 0.01f;
        }
    }
}
