using System;
using UnityEngine;

public class MonsterManager : InGameManager
{
    public event Action OnMonsterKilled;
    public event Action OnFieldCleared;

    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private int gridRows = 5;
    [SerializeField] private int gridCols = 5;
    [SerializeField] private float slotSpacing = 1f;

    private MonsterSpawner spawner;
    private MonsterField field;
    private Vector3 fieldOrigin;

    public override void Initialize()
    {
        base.Initialize();
        spawner = new MonsterSpawner(GameManager.ObjectPool, monsterPrefab);
        field = new MonsterField(gridRows, gridCols);
        CalculateFieldOrigin();
    }

    private void CalculateFieldOrigin()
    {
        Camera cam = Camera.main;
        float camZ = Mathf.Abs(cam.transform.position.z);
        Vector3 topRight = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, camZ));

        float gridWidth = (gridCols - 1) * slotSpacing;
        fieldOrigin = new Vector3(-gridWidth / 2f, topRight.y - slotSpacing, 0f);
    }

    public void SpawnWave(int monsterCount, int maxHp)
    {
        for (int i = 0; i < monsterCount; i++)
        {
            var (row, col) = field.GetNextEmptySlot();
            if (row < 0) break; // TODO: 필드 꽉 찼을 때 처리 방식 결정

            Vector3 position = GetWorldPosition(row, col);
            Monster monster = spawner.Spawn(position, maxHp);
            field.Add(monster, row, col);
            monster.OnDied += HandleMonsterDied;
        }
    }

    private void HandleMonsterDied(Monster monster)
    {
        monster.OnDied -= HandleMonsterDied;
        field.Remove(monster);
        spawner.Release(monster);

        OnMonsterKilled?.Invoke();
        if (field.IsEmpty)
            OnFieldCleared?.Invoke();
    }

    private Vector3 GetWorldPosition(int row, int col)
    {
        return fieldOrigin + new Vector3(col * slotSpacing, -row * slotSpacing, 0f);
    }
}
