using System.Collections.Generic;

// 활성 몬스터 목록의 주인 — 추가/제거/전멸 판정만 (순수 C#)
public class MonsterField
{
    private readonly List<Monster> activeMonsters = new();

    public bool IsEmpty => activeMonsters.Count == 0;
    public IReadOnlyList<Monster> ActiveMonsters => activeMonsters;

    public void Add(Monster monster) => activeMonsters.Add(monster);
    public void Remove(Monster monster) => activeMonsters.Remove(monster);
    public void Clear() => activeMonsters.Clear();
}
