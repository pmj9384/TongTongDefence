using System;
using System.Collections;
using UnityEngine;

public class MonsterManager : InGameManager
{
    public event Action OnMonsterKilled;
    public event Action OnFieldCleared;

    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private int laneCount = 9;          // 가로 9열
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float rowInterval = 0.8f;   // 다음 행이 내려올 때까지 간격(하강에 맞춰 튜닝)
    [SerializeField] private float rowSpacing = 1f;      // 스폰 시 행 간 Y 간격(0이면 top에 겹쳐 스폰 후 드리프트로 벌어짐)

    private MonsterSpawner spawner;
    private MonsterField field;
    private float laneSpacing;
    private float spawnY;
    private float failY;
    private float laneStartX;
    private int remainingRows;

    public override void Initialize()
    {
        base.Initialize();
        spawner = new MonsterSpawner(GameManager.ObjectPool, monsterPrefab);
        field = new MonsterField(laneCount);
        CalculateFieldBounds();
    }

    private void CalculateFieldBounds()
    {
        FieldManager fm = GameManager.FieldManager;
        laneSpacing = (fm.RightWall - fm.LeftWall) / laneCount;
        // 각 레인의 "중심"에 배치: 왼쪽벽 + 반칸 + lane*칸
        laneStartX = fm.LeftWall + laneSpacing * 0.5f;
        spawnY = fm.TopWall - laneSpacing * 0.5f;
        failY = fm.BottomWall + laneSpacing * 0.5f; // 플레이어 라인 근처. 필요 시 Shooter 배치와 맞춰 미세조정
    }

    // WaveManager가 호출. monsterCount/maxHp는 유지하되 내부에서 행으로 쪼개 스폰.
    public void Spawn(int monsterCount, int maxHp)
    {
        StartCoroutine(SpawnRoutine(monsterCount, maxHp));
    }

    private IEnumerator SpawnRoutine(int monsterCount, int maxHp)
    {
        // monsterCount를 laneCount로 나눠 행 개수 산출. 마지막 행은 남는 수만큼만 채움.
        int fullRows = monsterCount / laneCount;
        int remainder = monsterCount % laneCount;
        remainingRows = fullRows + (remainder > 0 ? 1 : 0);

        for (int r = 0; r < fullRows; r++)
        {
            SpawnRow(laneCount, maxHp);
            remainingRows--;
            yield return new WaitForSeconds(rowInterval);
        }
        if (remainder > 0)
        {
            SpawnRow(remainder, maxHp);
            remainingRows--;
        }
    }

    // countInRow 개의 몬스터를 같은 Y(spawnY)에 서로 다른 레인으로 스폰.
    // 어느 레인을 채울지는 지금은 0..count-1(왼쪽부터). 정확한 대형 모양은 가산점으로 미룸.
    private void SpawnRow(int countInRow, int maxHp)
    {
        for (int lane = 0; lane < countInRow; lane++)
        {
            Vector3 position = new Vector3(laneStartX + lane * laneSpacing, spawnY, 0f);
            Monster monster = spawner.Spawn(position, maxHp);
            MonsterMover mover = monster.GetComponent<MonsterMover>();
            mover.Initialize(moveSpeed, failY);
            mover.OnReachedBottom += HandleMonsterReachedBottom;
            field.Add(monster);
            monster.OnDied += HandleMonsterDied;
        }
    }

    private void HandleMonsterDied(Monster monster)
    {
        monster.OnDied -= HandleMonsterDied;
        monster.GetComponent<MonsterMover>().OnReachedBottom -= HandleMonsterReachedBottom;
        field.Remove(monster);
        spawner.Release(monster);

        OnMonsterKilled?.Invoke();
        if (field.IsEmpty && remainingRows == 0) // 스폰 도중 조기 발화 방지 (기존 remainingToSpawn 가드의 행 버전)
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
        StopAllCoroutines();
        remainingRows = 0;
        foreach (Monster monster in field.ActiveMonsters)
        {
            monster.OnDied -= HandleMonsterDied;
            monster.GetComponent<MonsterMover>().OnReachedBottom -= HandleMonsterReachedBottom;
            spawner.Release(monster);
        }
        field.Clear();
    }
}
