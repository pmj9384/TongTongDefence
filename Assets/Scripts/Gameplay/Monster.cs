using System;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public event Action<Monster> OnDied;
    public event Action<Monster, int, bool> OnDamaged;   // (자신, 가해진 데미지, 치명타) — 데미지 팝업용
    public event Action<int, int> OnHpChanged;           // (현재, 최대) — 체력바용

    public int CurrentHp { get; private set; }
    public int MaxHp { get; private set; }

    public void Initialize(int maxHp)
    {
        MaxHp = maxHp;
        CurrentHp = maxHp;
    }

    // 데미지 "적용"만 — 계산은 SkillManager/DamageCalculator 몫 (첫 설계 원칙)
    public void TakeDamage(int damage, bool isCritical)
    {
        if (CurrentHp <= 0) return;

        CurrentHp = Mathf.Max(0, CurrentHp - damage);
        OnDamaged?.Invoke(this, damage, isCritical);
        OnHpChanged?.Invoke(CurrentHp, MaxHp);
        if (CurrentHp == 0)
            OnDied?.Invoke(this);
    }
}
