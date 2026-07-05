using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : InGameManager
{
    public event Action<Monster> OnMonsterKilled;   // 처치된 몬스터 전달 (레벨 카운트·치명타 1회 소모 해제·성냥 폭발 위치)
    public event Action OnFieldCleared;

    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private float moveSpeed = 0.2f;     // 필드 높이 기준 하강 속도
    [SerializeField] private MonsterTypeData[] types;    // 몬스터 4종 구성 (Inspector 수동 연결)

    private MonsterSpawner spawner;
    private MonsterField field;
    private FieldManager fieldManager;   // 격자 지오메트리의 주인 — 위치는 전부 CellToWorld로 지목
    private float failY;

    // 사망 몬스터의 풀 반환 대기 목록 — 물리 콜백 "도중" 콜라이더를 끄면 볼의 반사 계산이
    // 반쪽으로 끝나 속도가 0이 되는 버그가 있어, 물리적 제거만 프레임 끝(LateUpdate)으로 미룬다.
    // (논리적 죽음 — 킬 카운트/웨이브 판정 — 은 즉시)
    private readonly List<Monster> pendingRelease = new();

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

    // 레이저볼 "같은 행" 쿼리 — 기준 Y에서 반 칸 이내의 활성 몬스터 (연속 하강이라 행 인덱스보다 Y밴드가 정확)
    public List<Monster> GetMonstersNearRow(float y)
    {
        var result = new List<Monster>();
        float halfCell = fieldManager.CellHeight * 0.5f;
        foreach (Monster m in field.ActiveMonsters)
            if (Mathf.Abs(m.transform.position.y - y) <= halfCell)
                result.Add(m);
        return result;
    }

    private void HandleMonsterDied(Monster monster)
    {
        monster.OnDied -= HandleMonsterDied;
        monster.GetComponent<MonsterMover>().OnReachedBottom -= HandleMonsterReachedBottom;
        field.Remove(monster);
        pendingRelease.Add(monster);   // 물리적 제거는 LateUpdate에서

        OnMonsterKilled?.Invoke(monster);
        if (field.IsEmpty)   // 동기 스폰이라 스폰 도중 조기 발화 경합이 없음 (코루틴/가드 삭제됨)
            OnFieldCleared?.Invoke();
    }

    // 이 프레임의 모든 물리 스텝/충돌 콜백이 끝난 뒤 — 볼 반사가 온전히 완료된 다음 실제로 끈다
    private void LateUpdate()
    {
        if (pendingRelease.Count == 0) return;
        foreach (Monster monster in pendingRelease)
            spawner.Release(monster);
        pendingRelease.Clear();
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
