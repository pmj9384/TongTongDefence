using System.Collections.Generic;

public class MonsterField
{
    private readonly int rows;
    private readonly int cols;
    private readonly Monster[,] grid;
    private readonly Dictionary<Monster, (int row, int col)> positions = new();

    public bool IsEmpty { get; private set; } = true;

    public MonsterField(int rows, int cols)
    {
        this.rows = rows;
        this.cols = cols;
        grid = new Monster[rows, cols];
    }

    public (int row, int col) GetNextEmptySlot()
    {
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                if (grid[r, c] == null)
                    return (r, c);
        return (-1, -1);
    }

    public void Add(Monster monster, int row, int col)
    {
        grid[row, col] = monster;
        positions[monster] = (row, col);
        IsEmpty = false;
    }

    public void Remove(Monster monster)
    {
        var (row, col) = positions[monster];
        grid[row, col] = null;
        positions.Remove(monster);
        IsEmpty = CheckEmpty();
    }

    private bool CheckEmpty()
    {
        foreach (var m in grid)
            if (m != null) return false;
        return true;
    }

    public void AdvanceRow()
    {
        // TODO: 모든 몬스터 row-1, 0행 도달 시 실패 조건 — 실제 실패 판정은 MonsterManager/GameManager 몫
    }
}
