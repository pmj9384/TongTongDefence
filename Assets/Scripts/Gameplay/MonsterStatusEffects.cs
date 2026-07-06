using System.Collections.Generic;
using UnityEngine;

// 몬스터의 "시간성 상태이상"만 담당 — 화상(도트 중첩), 냉동(감속+받피증).
// 플레이어 스탯(영구 성장)과 성격이 달라 분리. 풀 반환(OnDisable) 시 완전 초기화가 핵심.
// Update 기반이라 timeScale 정지(스킬 선택) 중엔 자동으로 멈춘다.
public class MonsterStatusEffects : MonoBehaviour
{
    public bool IsFrozen => freezeRemaining > 0f;
    public float FrozenDamageBonus { get; private set; }

    private struct BurnStack { public float remaining; public float dps; }

    private readonly List<BurnStack> burnStacks = new();
    private float burnTickTimer;

    private float freezeRemaining;
    private Monster monster;
    private MonsterMover mover;

    private void Awake()
    {
        monster = GetComponent<Monster>();
        mover = GetComponent<MonsterMover>();
    }

    // 화상 부여 — 스택 추가(상한 초과 시 가장 오래된 스택 교체), 스택별 초당 피해는 부여 시점 레벨 기준
    public void ApplyBurn(float duration, int maxStacks, float damagePerSecond)
    {
        if (burnStacks.Count >= maxStacks)
            burnStacks.RemoveAt(0);
        burnStacks.Add(new BurnStack { remaining = duration, dps = damagePerSecond });
    }

    // 냉동 부여 — 재적용 시 타이머/수치 갱신
    public void ApplyFreeze(float duration, float slowRate, float damageBonus)
    {
        freezeRemaining = duration;
        FrozenDamageBonus = damageBonus;
        mover.SpeedMultiplier = 1f - slowRate;
    }

    private void Update()
    {
        TickBurn();
        TickFreeze();
    }

    private void TickBurn()
    {
        if (burnStacks.Count == 0) { burnTickTimer = 0f; return; }   // 스택 소진 시 리셋 — 다음 화상 첫 틱 조기 방지

        float dt = Time.deltaTime;
        float totalDps = 0f;
        for (int i = burnStacks.Count - 1; i >= 0; i--)
        {
            BurnStack stack = burnStacks[i];
            stack.remaining -= dt;
            if (stack.remaining <= 0f) { burnStacks.RemoveAt(i); continue; }
            burnStacks[i] = stack;
            totalDps += stack.dps;
        }

        burnTickTimer += dt;
        if (burnTickTimer >= 1f && totalDps > 0f)   // 1초마다 "중첩 수 × 중첩당 피해"
        {
            burnTickTimer -= 1f;
            monster.TakeDamage(Mathf.RoundToInt(totalDps), false);   // 도트는 치명타/증폭 미적용 [가정5]
        }
    }

    private void TickFreeze()
    {
        if (freezeRemaining <= 0f) return;

        freezeRemaining -= Time.deltaTime;
        if (freezeRemaining <= 0f)
        {
            FrozenDamageBonus = 0f;
            mover.SpeedMultiplier = 1f;
        }
    }

    // 풀 반환 시 상태 완전 초기화 — 재사용된 몬스터에 화상/냉동이 남으면 안 됨
    private void OnDisable()
    {
        burnStacks.Clear();
        burnTickTimer = 0f;
        freezeRemaining = 0f;
        FrozenDamageBonus = 0f;
        if (mover != null) mover.SpeedMultiplier = 1f;
    }
}
