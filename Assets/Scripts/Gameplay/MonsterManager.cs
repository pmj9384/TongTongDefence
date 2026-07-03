using System;
using System.Collections;
using UnityEngine;

public class MonsterManager : InGameManager
{
    public event Action OnMonsterKilled;
    public event Action OnFieldCleared;

    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private float moveSpeed = 0.2f;     // 필드 높이 기준 하강 속도
    [SerializeField] private float rowInterval = 0.8f;   // 다음 행이 내려올 때까지 간격(하강에 맞춰 튜닝)

    private MonsterSpawner spawner;
    private MonsterField field;
    private FieldManager fieldManager;   // 격자 지오메트리의 주인 — 위치는 전부 CellToWorld로 지목
    private float failY;
    private float monsterScale;
    private int remainingRows;

    public override void Initialize()
    {
        base.Initialize();
        fieldManager = GameManager.FieldManager;
        spawner = new MonsterSpawner(GameManager.ObjectPool, monsterPrefab);
        field = new MonsterField(fieldManager.Columns);

        failY = fieldManager.BottomWall + fieldManager.CellHeight * 0.5f; // 판 바닥 반 칸 위. 필요 시 Shooter 배치와 맞춰 미세조정
        // 셀 크기가 기기 화면비 의존이라 몬스터 스케일은 런타임 계산 (셀의 90% 채움)
        float monsterBaseWidth = monsterPrefab.GetComponentInChildren<SpriteRenderer>().sprite.bounds.size.x;
        monsterScale = fieldManager.CellWidth * 0.9f / monsterBaseWidth;
    }

    // WaveManager가 호출. monsterCount/maxHp는 유지하되 내부에서 행으로 쪼개 스폰.
    public void Spawn(int monsterCount, int maxHp)
    {
        StartCoroutine(SpawnRoutine(monsterCount, maxHp));
    }

    private IEnumerator SpawnRoutine(int monsterCount, int maxHp)
    {
        // monsterCount를 열 수로 나눠 행 개수 산출. 마지막 행은 남는 수만큼만 채움.
        int fullRows = monsterCount / fieldManager.Columns;
        int remainder = monsterCount % fieldManager.Columns;
        remainingRows = fullRows + (remainder > 0 ? 1 : 0);

        for (int r = 0; r < fullRows; r++)
        {
            SpawnRow(fieldManager.Columns, maxHp);
            remainingRows--;
            yield return new WaitForSeconds(rowInterval);
        }
        if (remainder > 0)
        {
            SpawnRow(remainder, maxHp);
            remainingRows--;
        }
    }

    // countInRow 개의 몬스터를 0행(스폰줄)의 서로 다른 열에 스폰.
    // 어느 레인을 채울지는 지금은 0..count-1(왼쪽부터). 정확한 대형 모양은 가산점으로 미룸.
    private void SpawnRow(int countInRow, int maxHp)
    {
        for (int lane = 0; lane < countInRow; lane++)
        {
            Vector3 position = fieldManager.CellToWorld(row: 0, col: lane);
            Monster monster = spawner.Spawn(position, maxHp);
            monster.transform.localScale = Vector3.one * monsterScale;
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
