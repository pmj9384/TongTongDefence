using System;
using System.Collections;
using UnityEngine;

// 행 컨베이어 (원작 확정 관찰 2026-07-07): 시작 5행 일괄 → "앞 행이 한 칸 내려올 때마다" 다음 행 스폰.
// 행 간격을 시간이 아니라 거리(한 칸)로 정의 — 줄들이 격자 간격을 유지하고, 세로 1×2의 위 칸 점유가 정합된다.
// 빈 행도 패턴의 일부(그룹 간격). 클리어 = 행 소진 + 필드 전멸. 전멸해도 다음 행은 제 박자에 온다 [가정].
public class WaveManager : InGameManager
{
    public event Action OnWaveAllClear;

    // 진행도(처치 비율)의 분모 — 패턴의 총 유닛 수 (멀티셀도 1유닛)
    public int TotalMonsterCount => pattern.TotalUnits;

    [SerializeField] private int initialRows = 5;      // 시작 일괄 스폰 행 수 (원작 관찰)
    [SerializeField] private int baseHp = 30;          // 행 HP = baseHp + 행번호 × hpPerRow [튜닝]
    [SerializeField] private int hpPerRow = 10;

    private StagePattern pattern;
    private MonsterManager monsterManager;
    private int nextRow;
    private bool hasStarted;

    public override void Initialize()
    {
        base.Initialize();
        monsterManager = GameManager.MonsterManager;

        var csv = Resources.Load<TextAsset>("Tables/StagePattern");
        pattern = StagePattern.Parse(csv.text, GameManager.FieldManager.Columns);   // 엄격 파서 — 오염 즉시 예외

        monsterManager.OnFieldCleared += HandleFieldCleared;
        GameManager.AddGameStateEnterAction(GameManager.GameState.GamePlay, TryStart);
    }

    public override void Clear()
    {
        base.Clear();
        StopAllCoroutines();
        monsterManager.OnFieldCleared -= HandleFieldCleared;
        GameManager.RemoveGameStateEnterAction(GameManager.GameState.GamePlay, TryStart);
    }

    private void TryStart()
    {
        if (hasStarted) return;   // SkillSelection 복귀 등 GamePlay 재진입 대비
        hasStarted = true;

        // 시작 일괄: 패턴 첫 행이 가장 아래 (먼저 나온 행이 먼저 내려가던 상태)
        int batch = Mathf.Min(initialRows, pattern.Rows.Count);
        for (int i = 0; i < batch; i++)
            monsterManager.SpawnRow(pattern.Rows[i], RowHp(i), gridRowOffset: batch - 1 - i);
        nextRow = batch;

        StartCoroutine(ConveyorLoop());
    }

    private IEnumerator ConveyorLoop()
    {
        // "한 칸 내려오는 시간"마다 다음 행 — 스케일 시간이라 정지(선택/퍼즈) 동안 이동과 함께 멈춰 간격 유지
        float interval = GameManager.FieldManager.CellHeight / monsterManager.MoveSpeed;
        var wait = new WaitForSeconds(interval);

        while (nextRow < pattern.Rows.Count)
        {
            yield return wait;
            monsterManager.SpawnRow(pattern.Rows[nextRow], RowHp(nextRow));
            nextRow++;
        }
    }

    private int RowHp(int rowIndex) => baseHp + rowIndex * hpPerRow;

    // 필드 전멸 — 행이 남아 있으면 무시 (다음 행이 제 박자에 옴), 소진됐으면 클리어
    private void HandleFieldCleared()
    {
        if (nextRow < pattern.Rows.Count) return;

        OnWaveAllClear?.Invoke();
        GameManager.SetGameState(GameManager.GameState.GameClear);
    }
}
