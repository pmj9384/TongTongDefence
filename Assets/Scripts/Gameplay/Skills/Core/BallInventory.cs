using System.Collections.Generic;

// 발사 볼 인벤토리 — "보유 볼 목록 + 발사 대기열"만 담당 (순수 C#, 테스트 대상).
// 원작 관찰(2026-07-06): 보유 볼 각각이 실제 개체로 필드에 나가 있거나 대기 중이며,
// 먼저 회수된 볼부터 재발사된다 → Queue가 그 규칙 자체다. 쿨다운이 아니라 "회수가 발사를 만든다".
public class BallInventory
{
    private readonly Queue<SkillId?> ready = new();   // null = 노멀볼

    public int TotalCount { get; private set; }   // 보유 총수 (필드+대기)
    public int ReadyCount => ready.Count;

    // 볼 추가 (노멀 = null) — 새 볼은 즉시 발사 대기열에 합류
    public void Add(SkillId? skill)
    {
        TotalCount++;
        ready.Enqueue(skill);
    }

    // 대기열 앞의 볼을 꺼냄 (발사) — 대기 없으면 false (전부 필드에 나가 있는 상태)
    public bool TryTakeNext(out SkillId? skill)
    {
        if (ready.Count == 0)
        {
            skill = null;
            return false;
        }
        skill = ready.Dequeue();
        return true;
    }

    // 회수된 볼을 대기열 뒤로 — 먼저 회수된 순으로 재발사되는 원작 규칙
    public void Return(SkillId? skill) => ready.Enqueue(skill);
}
