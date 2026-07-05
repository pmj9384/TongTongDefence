using System.Collections.Generic;

public class MonsterField
{
    private readonly int laneCount;
    private readonly List<Monster> activeMonsters = new();
    private int nextLane;

    public bool IsEmpty => activeMonsters.Count == 0;
    public IReadOnlyList<Monster> ActiveMonsters => activeMonsters;

    public MonsterField(int laneCount)
    {
        this.laneCount = laneCount;
    }

    public int GetNextLane()
    {
        int lane = nextLane;
        nextLane = (nextLane + 1) % laneCount;
        return lane;
    }

    public void Add(Monster monster) => activeMonsters.Add(monster);
    public void Remove(Monster monster) => activeMonsters.Remove(monster);
    public void Clear() => activeMonsters.Clear();
}
