using System;
using System.Collections;
using UnityEngine;

// 웨이브 진행만 담당: 시작 → 전멸 시 다음 웨이브 스폰 → 마지막 클리어 시 GameClear.
// 처치 수 카운팅/3택지 발동은 SkillManager(PlayerLevel)의 몫 — 여기서 하지 않는다.
public class WaveManager : InGameManager
{
    public event Action OnWaveAllClear;

    // 비워두면 Initialize에서 공식으로 자동 생성. Inspector에 넣으면 그게 우선
    [SerializeField] private WaveData[] waves;
    [SerializeField] private float nextWaveDelay = 0.8f;   // 전멸 → 다음 웨이브 텀 (원작 재현 + 캐스케이드 분리)

    private MonsterManager monsterManager;
    private int currentWave;
    private bool hasStarted;

    public override void Initialize()
    {
        base.Initialize();
        // 매니저 간 참조는 GameManager 프로퍼티 경유 — MonsterManager가 FieldManager를 읽는 것과 동일 패턴.
        // (GameManager가 모든 매니저 프로퍼티 할당을 끝낸 뒤 Initialize 루프를 돌므로 순서 안전)
        monsterManager = GameManager.MonsterManager;

        if (waves == null || waves.Length == 0)
            waves = GenerateDefaultWaves();

        monsterManager.OnFieldCleared += HandleFieldCleared;
        GameManager.AddGameStateEnterAction(GameManager.GameState.GamePlay, TryStartFirstWave);
    }

    public override void Clear()
    {
        base.Clear();
        StopAllCoroutines();   // 지연 스폰 중 씬 정리 대비
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
        monsterManager.Spawn(waves[waveIndex].monsterCount, waves[waveIndex].monsterHp, waveIndex);
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
            // 즉시 스폰 금지 — 사망 처리(레이저 행/폭발 연쇄) "도중에" 같은 스택에서 새 웨이브가 스폰되면
            // 진행 중인 캐스케이드가 새 몬스터를 즉시 타격해 무한 킬 체인이 됨 (실버그).
            // 딜레이는 timeScale을 타므로 스킬 선택 중이면 선택이 끝난 뒤 스폰된다.
            StartCoroutine(StartWaveDelayed(currentWave + 1));
        }
    }

    private IEnumerator StartWaveDelayed(int waveIndex)
    {
        yield return new WaitForSeconds(nextWaveDelay);
        StartWave(waveIndex);
    }

    // 결정적 공식 — 매판 동일 구성 (원작 관찰: 정해진 웨이브). Random 사용 금지
    private WaveData[] GenerateDefaultWaves()
    {
        var result = new WaveData[20];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = new WaveData
            {
                monsterCount = 5 + i,
                monsterHp = 10 + i * 4,
            };
        }
        return result;
    }
}
