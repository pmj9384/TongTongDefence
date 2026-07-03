using System;
using UnityEngine;

public class MonsterManager : InGameManager
{
    public event Action OnMonsterKilled;
    public event Action OnFieldCleared;

    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private float moveSpeed = 0.2f;     // 필드 높이 기준 하강 속도
    [SerializeField] private MonsterTypeData[] types;    // 몬스터 4종 구성 (Inspector 수동 연결)

    private MonsterSpawner spawner;
    private MonsterField field;
    private FieldManager fieldManager;   // 격자 지오메트리의 주인 — 위치는 전부 CellToWorld로 지목
    private float failY;

    public override void Initialize()
    {
        base.Initialize();
        fieldManager = GameManager.FieldManager;
        spawner = new MonsterSpawner(GameManager.ObjectPool, monsterPrefab);
        field = new MonsterField(fieldManager.Columns);
        failY = fieldManager.BottomWall + fieldManager.CellHeight * 0.5f; // 판 바닥 반 칸 위. 필요 시 Shooter 배치와 맞춰 미세조정
    }

    // 웨이브 전원 동시 스폰 (원작 관찰: 한 번에 등장, 매판 동일 배치 — 결정적, Random 금지)
    public void Spawn(int monsterCount, int maxHp, int waveIndex)
    {
        for (int i = 0; i < monsterCount; i++)
        {
            int row = i / fieldManager.Columns;
            int col = i % fieldManager.Columns;
            MonsterTypeData type = types[(waveIndex + i) % types.Length];

            Monster monster = spawner.Spawn(fieldManager.CellToWorld(row, col),
                                            Mathf.RoundToInt(maxHp * type.hpMultiplier));
            // 블록 스프라이트가 1월드유닛으로 임포트돼 있어 스케일 = 셀 폭이면 블록이 칸에 꽉 참
            monster.transform.localScale = Vector3.one * fieldManager.CellWidth;
            monster.GetComponent<MonsterVisual>().Apply(type);

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
        if (field.IsEmpty)   // 동기 스폰이라 스폰 도중 조기 발화 경합이 없음 (코루틴/가드 삭제됨)
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
