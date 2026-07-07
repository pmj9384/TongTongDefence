using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        WaitLoading,
        GameReady,
        GamePlay,
        GameStop,
        SkillSelection,   // 3택지 스킬 선택 — 게임 완전 정지 (원작 관찰 확정)
        GameOver,
        GameClear,
        Max,
    }

    // 진입(Enter) -> 시작(Start) -> 퇴장(Exit) 순으로 실행
    private Action[] gameStateEnterAction;
    private Action[] gameStateStartAction;
    private Action[] gameStateExitAction;

    private GameState previousState;
    private GameState currentState;
    public GameState CurrentState => currentState;
    public GameState PreviousState => previousState;

    private float previousStopTimeScale;

    #region 핵심 매니저 시스템
    private List<IManager> managers = new List<IManager>();

    public ObjectPoolManager ObjectPool { get; private set; }
    public FieldManager FieldManager { get; private set; }
    public GameUIManager UIManager { get; private set; }
    public BallManager BallManager { get; private set; }
    public MonsterManager MonsterManager { get; private set; }
    public WaveManager WaveManager { get; private set; }
    public SkillManager SkillManager { get; private set; }
    public PlayerManager PlayerManager { get; private set; }
    public StatsManager StatsManager { get; private set; }
    // TODO: 게임별 매니저 추가

    #endregion

    private void Awake()
    {
        SetInitialSettings();
        InitializeStateActions();
        InitializeCoreManagers();
        SetGameState(GameState.WaitLoading);

        // TEMP: 아웃게임 씬이 아직 없어서 임시로 바로 플레이 진입. 아웃게임 추가되면 제거.
        SkipTitle = true;
    }

    public static bool SkipTitle;

    private void Start()
    {
        SetGameState(GameState.GameReady);
        if (SkipTitle)
        {
            SkipTitle = false;
            SetGameState(GameState.GamePlay);
        }
    }

    private void SetInitialSettings()
    {
#if UNITY_EDITOR
        Application.targetFrameRate = -1;
#else
        Application.targetFrameRate = 60;
#endif
    }

    private void InitializeStateActions()
    {
        int stateCount = (int)GameState.Max;
        gameStateEnterAction = new Action[stateCount];
        gameStateStartAction = new Action[stateCount];
        gameStateExitAction = new Action[stateCount];

        // 기본 일시정지/재생
        AddGameStateStartAction(GameState.GameStop, PauseTimeScale);
        AddGameStateExitAction(GameState.GameStop, ResumeTimeScale);

        // 스킬 선택 중 완전 정지 — GameStop과 동일 패턴 (몬스터 하강·볼·쿨다운 전부 멈춤)
        AddGameStateStartAction(GameState.SkillSelection, PauseTimeScale);
        AddGameStateExitAction(GameState.SkillSelection, ResumeTimeScale);

        // 결과 상태도 완전 정지 — 예약된 웨이브 코루틴(WaitForSeconds)·발사·하강이 결과 화면 뒤에서
        // 계속 돌던 버그. 재시작은 RestartGame이 timeScale=1 복원 후 씬 리로드라 Exit 액션 불필요
        AddGameStateStartAction(GameState.GameOver, PauseTimeScale);
        AddGameStateStartAction(GameState.GameClear, PauseTimeScale);

        // BGM 연결 - BgmClipId는 프로젝트마다 Defines/Enums.cs에 정의 필요
        // AddGameStateEnterAction(GameState.GameReady, () => SoundManager.Instance.PlayBgm(BgmClipId.IngameBGM));
        AddGameStateEnterAction(GameState.GameStop, () => SoundManager.Instance.PauseBgm());
        AddGameStateEnterAction(GameState.GameStop, () => SoundManager.Instance.PauseSfx());
        AddGameStateExitAction(GameState.GameStop, () => SoundManager.Instance.ResumeBgm());
        AddGameStateExitAction(GameState.GameStop, () => SoundManager.Instance.ResumeSfx());
        AddGameStateEnterAction(GameState.GameOver, () => SoundManager.Instance.StopBgm());

        // TODO: GameOver 시 점수 저장 등 게임 특화 로직 추가
        // AddGameStateEnterAction(GameState.GameOver, () => GameDataManager.Instance.PlayerAccountData.TryUpdateBestScore(score));
    }

    private void InitializeCoreManagers()
    {
        // 1. 순수 C# 매니저
        ObjectPool = new ObjectPoolManager();
        managers.Add(ObjectPool);

        // 2. 씬에서 "Manager" 태그로 MonoBehaviour 매니저 자동 등록
        List<GameObject> managerObjects = GameObject.FindGameObjectsWithTag("Manager").ToList();

        // FieldManager를 먼저 등록 — 다른 매니저의 Initialize()가 경계값을 읽을 수 있음
        FieldManager = RegisterManager<FieldManager>(managerObjects);
        UIManager = RegisterManager<GameUIManager>(managerObjects);
        BallManager = RegisterManager<BallManager>(managerObjects);
        MonsterManager = RegisterManager<MonsterManager>(managerObjects);
        WaveManager = RegisterManager<WaveManager>(managerObjects);
        SkillManager = RegisterManager<SkillManager>(managerObjects);
        PlayerManager = RegisterManager<PlayerManager>(managerObjects);
        StatsManager = RegisterManager<StatsManager>(managerObjects);
        // TODO: 게임별 매니저 등록 추가

        foreach (var manager in managers)
        {
            manager.Initialize();
        }
        UIManager.InitializedUIElements();
    }

    private T RegisterManager<T>(List<GameObject> list) where T : InGameManager
    {
        T component = null;
        foreach (var obj in list)
        {
            if (obj.TryGetComponent<T>(out component)) break;
        }

        if (component != null)
        {
            component.SetGameManager(this);
            managers.Add(component);
        }
        else
        {
            Debug.LogWarning($"[GameManager] {typeof(T).Name}를 찾을 수 없습니다.");
        }
        return component;
    }

    #region 상태 제어
    public void SetGameState(GameState newState)
    {
        if (currentState == newState) return;

        previousState = currentState;
        currentState = newState;

        gameStateExitAction[(int)previousState]?.Invoke();
        gameStateEnterAction[(int)currentState]?.Invoke();
        gameStateStartAction[(int)currentState]?.Invoke();
    }

    public void RestartGame()
    {
        SkipTitle = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void AddGameStateEnterAction(GameState state, Action action) => gameStateEnterAction[(int)state] += action;
    public void RemoveGameStateEnterAction(GameState state, Action action) => gameStateEnterAction[(int)state] -= action;
    public void AddGameStateStartAction(GameState state, Action action) => gameStateStartAction[(int)state] += action;
    public void RemoveGameStateStartAction(GameState state, Action action) => gameStateStartAction[(int)state] -= action;
    public void AddGameStateExitAction(GameState state, Action action) => gameStateExitAction[(int)state] += action;
    public void RemoveGameStateExitAction(GameState state, Action action) => gameStateExitAction[(int)state] -= action;
    #endregion

    private void PauseTimeScale()
    {
        previousStopTimeScale = Time.timeScale;
        Time.timeScale = 0;
    }

    private void ResumeTimeScale()
    {
        Time.timeScale = previousStopTimeScale;
    }

    private void OnDestroy()
    {
        foreach (var manager in managers)
        {
            // 종료 시 파괴 순서가 비결정적 — 이미 파괴된 매니저는 스킵.
            // IManager 목록이라 Unity의 == 오버로드가 안 걸리므로 UnityEngine.Object로 캐스팅해 판정
            if (manager is UnityEngine.Object unityManager && unityManager == null) continue;
            manager.Clear();
        }
    }
}
