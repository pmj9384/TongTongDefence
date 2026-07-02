using System;
using System.Collections;
using UnityEngine;

public class MonsterManager : InGameManager
{
    public event Action OnMonsterKilled;
    public event Action OnFieldCleared;

    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private int laneCount = 5;
    [SerializeField] private float laneSpacing = 1f;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float spawnInterval = 0.2f;

    private MonsterSpawner spawner;
    private MonsterField field;
    private float spawnY;
    private float failY;
    private float laneStartX;

    public override void Initialize()
    {
        base.Initialize();
        spawner = new MonsterSpawner(GameManager.ObjectPool, monsterPrefab);
        field = new MonsterField(laneCount);
        CalculateFieldBounds();
    }

    private void CalculateFieldBounds()
    {
        FieldManager fieldManager = GameManager.FieldManager;
        float gridWidth = (laneCount - 1) * laneSpacing;
        laneStartX = (fieldManager.LeftWall + fieldManager.RightWall) / 2f - gridWidth / 2f;
        spawnY = fieldManager.TopWall - laneSpacing;
        failY = fieldManager.BottomWall + laneSpacing; // TODO: 정확한 실패 임계값(플레이어 위치 기준)은 Shooter 배치와 맞춰 조정
    }

    public void Spawn(int monsterCount, int maxHp)
    {
        StartCoroutine(SpawnRoutine(monsterCount, maxHp));
    }

    private IEnumerator SpawnRoutine(int monsterCount, int maxHp)
    {
        for (int i = 0; i < monsterCount; i++)
        {
            SpawnOne(maxHp);
            if (i < monsterCount - 1)
                yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnOne(int maxHp)
    {
        int lane = field.GetNextLane();
        Vector3 position = new Vector3(laneStartX + lane * laneSpacing, spawnY, 0f);

        Monster monster = spawner.Spawn(position, maxHp);
        MonsterMover mover = monster.GetComponent<MonsterMover>();
        mover.Initialize(moveSpeed, failY);
        mover.OnReachedBottom += HandleMonsterReachedBottom;

        field.Add(monster);
        monster.OnDied += HandleMonsterDied;
    }

    private void HandleMonsterDied(Monster monster)
    {
        monster.OnDied -= HandleMonsterDied;
        monster.GetComponent<MonsterMover>().OnReachedBottom -= HandleMonsterReachedBottom;
        field.Remove(monster);
        spawner.Release(monster);

        OnMonsterKilled?.Invoke();
        if (field.IsEmpty)
            OnFieldCleared?.Invoke();
    }

    private void HandleMonsterReachedBottom(MonsterMover mover)
    {
        GameManager.SetGameState(GameManager.GameState.GameOver);
        ClearAllMonsters();
    }

    public override void Clear()
    {
        base.Clear();
        ClearAllMonsters();
    }

    private void ClearAllMonsters()
    {
        foreach (Monster monster in field.ActiveMonsters)
        {
            monster.OnDied -= HandleMonsterDied;
            monster.GetComponent<MonsterMover>().OnReachedBottom -= HandleMonsterReachedBottom;
            spawner.Release(monster);
        }
        field.Clear();
    }
}
