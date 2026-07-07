using System;

// 플레이어 체력 — 순수 C# (도달 몬스터의 공격으로 감소, 0이면 사망 → GameOver는 PlayerManager 몫).
// 원작 관찰: 슈터 아래 게이지, 예시값 300. 몬스터 도달 = 즉사가 아니라 체력전.
public class PlayerHealth
{
    public event Action<int, int> OnChanged;   // (현재, 최대)
    public event Action OnDied;

    public int Max { get; }
    public int Current { get; private set; }
    public bool IsDead => Current <= 0;

    public PlayerHealth(int max)
    {
        Max = max;
        Current = max;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead || damage <= 0) return;

        Current = Math.Max(0, Current - damage);
        OnChanged?.Invoke(Current, Max);
        if (Current == 0)
            OnDied?.Invoke();
    }
}
