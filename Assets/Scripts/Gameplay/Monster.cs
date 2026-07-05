using System;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public event Action<Monster> OnDied;

    public int CurrentHp { get; private set; }
    public int MaxHp { get; private set; }

    public void Initialize(int maxHp)
    {
        MaxHp = maxHp;
        CurrentHp = maxHp;
    }

    public void TakeDamage(int damage, bool isCritical)
    {
        if (CurrentHp <= 0) return;

        CurrentHp = Mathf.Max(0, CurrentHp - damage);
        if (CurrentHp == 0)
            OnDied?.Invoke(this);
    }
}
