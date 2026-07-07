using UnityEngine;

// 고스트볼 관통 감지 — 볼 본체는 GhostBall 레이어라 몬스터와 물리 충돌이 없으므로(통과),
// 이 자식 트리거(BallSensor 레이어 × Monster만 ON)가 지나치는 몬스터를 감지해 부모 Ball에 전달.
public class GhostSensor : MonoBehaviour
{
    [SerializeField] private Ball ball;

    private void OnTriggerEnter2D(Collider2D other) => ball.NotifyGhostHit(other);
}
