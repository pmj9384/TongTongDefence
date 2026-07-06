using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : InGameManager
{
    public event Action<Monster> OnMonsterKilled;   // 처치된 몬스터 전달 (레벨 카운트·치명타 1회 소모 해제·성냥 폭발 위치)
    public event Action<Monster> OnMonsterDespawned; // 처치가 아닌 소멸(도달 돌진) — 몬스터 단위 외부 기록 정리용 (단검 등)
    public event Action<SkillId?, int> OnDamageDealt; // (소스, 데미지) — 전투 정보 집계용 재방송 (StatsManager 구독)
    public event Action OnFieldCleared;

    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private GameObject damagePopupPrefab;   // TMP 데미지 플로터 (빌더 메뉴로 생성·연결)
    [SerializeField] private float moveSpeed = 0.2f;     // 필드 높이 기준 하강 속도
    [SerializeField] private MonsterTypeData[] types;    // 몬스터 4종 구성 (Inspector 수동 연결)
    [SerializeField] private int attackDamage = 10;      // 돌진 충돌 데미지 [가정 — 수치 재관찰]
    [SerializeField] private float chargeSpeed = 4f;     // 도달 후 플레이어 돌진 속도
    [SerializeField] private float chargeDelay = 3f;     // 도달 후 돌진까지 대기 (원작 관찰: 약 3초, 이때 처치 가능)
    [SerializeField] private int topSpawnRowOffset = 1;  // 스폰 시작 행 — 원작은 판 상단 한 행 아래부터 (완충 행:
                                                         // 웨이브 스폰이 상단의 볼과 겹쳐 벽 밖으로 밀어내던 실기기 버그의 원작식 해법)

    private MonsterSpawner spawner;
    private MonsterField field;
    private FieldManager fieldManager;   // 격자 지오메트리의 주인 — 위치는 전부 CellToWorld로 지목
    private float failY;

    // 사망 몬스터의 풀 반환 대기 목록 — 물리 콜백 "도중" 콜라이더를 끄면 볼의 반사 계산이
    // 반쪽으로 끝나 속도가 0이 되는 버그가 있어, 물리적 제거만 프레임 끝(LateUpdate)으로 미룬다.
    // (논리적 죽음 — 킬 카운트/웨이브 판정 — 은 즉시)
    private readonly List<Monster> pendingRelease = new();

    private DamagePopupSpawner popupSpawner;   // 데미지 플로터 (내부 컴포지션 — Spawner/Field와 동일 패턴)

    public override void Initialize()
    {
        base.Initialize();
        fieldManager = GameManager.FieldManager;
        spawner = new MonsterSpawner(GameManager.ObjectPool, monsterPrefab);
        field = new MonsterField();
        popupSpawner = new DamagePopupSpawner(GameManager.ObjectPool, damagePopupPrefab);
        failY = fieldManager.BottomWall + fieldManager.CellHeight * 0.5f; // 판 바닥 반 칸 위. 필요 시 Shooter 배치와 맞춰 미세조정

        // GameOver 진입 시 필드 정리 (기존엔 도달 즉시 GameOver+정리였으나 체력전으로 바뀌며 상태 액션으로 이동)
        GameManager.AddGameStateEnterAction(GameManager.GameState.GameOver, ClearAllMonsters);
    }

    public override void Clear()
    {
        base.Clear();
        GameManager.RemoveGameStateEnterAction(GameManager.GameState.GameOver, ClearAllMonsters);
        ClearAllMonsters();
    }

    // 웨이브 전원 동시 스폰 (원작 관찰: 한 번에 등장, 매판 동일 배치 — 결정적, Random 금지)
    public void Spawn(int monsterCount, int maxHp, int waveIndex)
    {
        for (int i = 0; i < monsterCount; i++)
        {
            int row = topSpawnRowOffset + i / fieldManager.Columns;   // 원작: 첫 줄 위 완충 행
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
            monster.OnDamaged += HandleMonsterDamaged;
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
        Unsubscribe(monster);
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

    // 바닥 도달 → 플레이어에게 돌진 (원작: 부딪히며 1회 데미지 주고 소멸. 멈춰서 지속 공격은 보스 전용)
    private void HandleMonsterReachedBottom(MonsterMover mover)
    {
        mover.OnReachedPlayer += HandleMonsterCrashed;
        mover.ChargeTo(GameManager.PlayerManager.Position, chargeSpeed, chargeDelay);
    }

    // 돌진 도착 — 플레이어 피격 + 몬스터 소멸. "처치"가 아니므로 OnMonsterKilled 미발행
    // (도달 소멸로 레벨업/성냥 폭발이 발동하면 안 됨). 필드가 비면 웨이브는 정상 진행.
    // 순서 주의: 몬스터 정리 먼저, 데미지(GameOver 전환 유발 가능)는 마지막 —
    // 전환 액션에서 예외가 나도 유령 몬스터(구독/필드 잔류)가 안 생기게 (실사례: GameUIManager 크래시)
    private void HandleMonsterCrashed(MonsterMover mover)
    {
        Monster monster = mover.GetComponent<Monster>();
        Unsubscribe(monster);
        field.Remove(monster);
        pendingRelease.Add(monster);
        OnMonsterDespawned?.Invoke(monster);   // 풀 재사용 대비 — 외부의 몬스터 단위 기록(단검 소모 등) 정리 신호

        popupSpawner.Show(GameManager.PlayerManager.Position + Vector2.up * 0.5f, attackDamage, false);
        GameManager.PlayerManager.TakeDamage(attackDamage);
        if (field.IsEmpty)
            OnFieldCleared?.Invoke();
    }

    // 데미지 플로터 — 모든 데미지 소스(볼/화상/레이저/파편/폭발)가 Monster 이벤트로 모이므로 여기서 일괄 표시.
    // 위치 = 몬스터 중앙 (원작 관찰 — 겹쳐도 그대로, 유저 확정)
    private void HandleMonsterDamaged(Monster monster, int damage, bool isCritical, SkillId? source)
    {
        popupSpawner.Show(monster.transform.position, damage, isCritical);
        OnDamageDealt?.Invoke(source, damage);
    }

    private void ClearAllMonsters()
    {
        foreach (Monster monster in field.ActiveMonsters)
        {
            if (monster == null) continue;   // 씬 언로드 시 몬스터가 매니저보다 먼저 Destroy되는 순서 대비
            Unsubscribe(monster);
            spawner.Release(monster);
        }
        field.Clear();
    }

    private void Unsubscribe(Monster monster)
    {
        monster.OnDied -= HandleMonsterDied;
        monster.OnDamaged -= HandleMonsterDamaged;
        MonsterMover mover = monster.GetComponent<MonsterMover>();
        mover.OnReachedBottom -= HandleMonsterReachedBottom;
        mover.OnReachedPlayer -= HandleMonsterCrashed;
    }
}
