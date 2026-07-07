using System;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public event Action<Monster> OnDied;
    // (자신, 가해진 데미지, 치명타, 소스) — 팝업 + 전투 정보 소스별 집계용. source null = 노멀볼
    public event Action<Monster, int, bool, SkillId?> OnDamaged;
    public event Action<int, int> OnHpChanged;           // (현재, 최대) — 체력바용

    public int CurrentHp { get; private set; }
    public int MaxHp { get; private set; }

    public void Initialize(int maxHp)
    {
        MaxHp = maxHp;
        CurrentHp = maxHp;
    }

    // 데미지 "적용"만 — 계산은 SkillManager/DamageCalculator 몫. source = 어느 스킬이 준 피해인지(집계용)
    public void TakeDamage(int damage, bool isCritical, SkillId? source = null)
    {
        if (CurrentHp <= 0) return;

        CurrentHp = Mathf.Max(0, CurrentHp - damage);
        OnDamaged?.Invoke(this, damage, isCritical, source);
        OnHpChanged?.Invoke(CurrentHp, MaxHp);
        if (CurrentHp == 0)
            OnDied?.Invoke(this);
    }
}
