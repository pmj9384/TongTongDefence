using UnityEngine;

// 플레이어 상태의 주인 — 체력 보유, 소진 시 GameOver 전환. (몬스터 도달 = 즉사가 아니라 체력전 — 원작 관찰)
public class PlayerManager : InGameManager
{
    [SerializeField] private int maxHealth = 300;        // 원작 게이지 관찰값
    [SerializeField] private Transform playerTransform;  // Shooter — 도달 몬스터의 돌진 목적지 (수동 연결: YAML)

    public PlayerHealth Health { get; private set; }
    public Vector2 Position => playerTransform.position;

    public override void Initialize()
    {
        base.Initialize();
        Health = new PlayerHealth(maxHealth);
        Health.OnDied += HandleDied;
    }

    public override void Clear()
    {
        base.Clear();
        Health.OnDied -= HandleDied;
    }

    public void TakeDamage(int damage) => Health.TakeDamage(damage);

    private void HandleDied()
    {
        GameManager.SetGameState(GameManager.GameState.GameOver);
    }
}
