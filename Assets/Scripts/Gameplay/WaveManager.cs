using System;
using UnityEngine;

public class WaveManager : InGameManager
{
    public event Action OnKillConditionMet;
    public event Action OnWaveAllClear;

    // 비워두면 Initialize에서 공식으로 자동 생성. Inspector에 넣으면 그게 우선
    [SerializeField] private WaveData[] waves;

    private MonsterManager monsterManager;
    private int currentWave;
    private int killCountThisWave;
    private bool hasStarted;

    public override void Initialize()
    {
        base.Initialize();
        // 매니저 간 참조는 GameManager 프로퍼티 경유 — MonsterManager가 FieldManager를 읽는 것과 동일 패턴.
        // (GameManager가 모든 매니저 프로퍼티 할당을 끝낸 뒤 Initialize 루프를 돌므로 순서 안전)
        monsterManager = GameManager.MonsterManager;

        if (waves == null || waves.Length == 0)
            waves = GenerateDefaultWaves();

        monsterManager.OnMonsterKilled += HandleMonsterKilled;
        monsterManager.OnFieldCleared += HandleFieldCleared;
        GameManager.AddGameStateEnterAction(GameManager.GameState.GamePlay, TryStartFirstWave);
    }

    public override void Clear()
    {
        base.Clear();
        monsterManager.OnMonsterKilled -= HandleMonsterKilled;
        monsterManager.OnFieldCleared -= HandleFieldCleared;
        GameManager.RemoveGameStateEnterAction(GameManager.GameState.GamePlay, TryStartFirstWave);
    }

    private void TryStartFirstWave()
    {
        if (hasStarted) return;   // SkillSelection 복귀 등 GamePlay 재진입 시 재시작 방지
        hasStarted = true;
        StartWave(0);
    }

    private void StartWave(int waveIndex)
    {
        currentWave = waveIndex;
        killCountThisWave = 0;
        monsterManager.Spawn(waves[waveIndex].monsterCount, waves[waveIndex].monsterHp);
    }

    private void HandleMonsterKilled()
    {
        killCountThisWave++;
        if (killCountThisWave == waves[currentWave].killCondition)
            OnKillConditionMet?.Invoke();   // 상태 전환(SkillSelection)은 스킬 플랜에서 — 여기선 발행만
    }

    private void HandleFieldCleared()
    {
        if (currentWave >= waves.Length - 1)
        {
            OnWaveAllClear?.Invoke();
            GameManager.SetGameState(GameManager.GameState.GameClear);
        }
        else
        {
            StartWave(currentWave + 1);
        }
    }

    // 결정적 공식 — 매판 동일 구성 (원작 관찰: 정해진 웨이브). Random 사용 금지
    private WaveData[] GenerateDefaultWaves()
    {
        var result = new WaveData[20];
        for (int i = 0; i < result.Length; i++)
        {
            int count = 5 + i;
            result[i] = new WaveData
            {
                monsterCount = count,
                monsterHp = 10 + i * 4,
                killCondition = count / 2,
            };
        }
        return result;
    }
}
