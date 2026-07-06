using UnityEngine;
using TMPro;
using UnityEngine.UI;

// 상단 HUD — 값 갱신 로직만. 배치/색/크기는 전부 씬(Inspector) 소관 (에디터-네이티브 전환, 2026-07-06).
// 텍스트는 변경 감지 캐시로 값이 바뀐 프레임에만 할당(GC 회피), 게이지는 목표값으로 부드럽게 차오름 (원작 관찰).
// UIElement 상속: 템플릿 의미는 "GameUIManager가 주입·관리하는 UI 단위" — 상시 표시라 Show/Hide는 미오버라이드
public class InGameHud : UIElement
{
    [Header("씬 참조 (TopBar 아래 조립됨)")]
    [SerializeField] private Slider progressSlider;   // 진행도 게이지 (핸들 없는 Slider — HP와 방식 통일)
    [SerializeField] private TMP_Text progressText;   // "N%"
    [SerializeField] private Slider levelSlider;      // 레벨(킬) 게이지
    [SerializeField] private TMP_Text levelText;      // "Lv.N"
    [SerializeField] private Button pauseButton;

    [SerializeField] private float gaugeFillSpeed = 1.5f;   // 게이지가 목표값까지 차오르는 속도(비율/초) [눈튜닝]

    private int lastProgressPercent = -1;
    private int lastLevel = -1;
    private int lastKillsIntoLevel = -1;
    private float progressTarget;
    private float levelTarget;
    private bool fillingToFull;   // 레벨업 연출: 게이지를 먼저 꽉 채우는 중

    private void Start()   // 매니저 Initialize 완료 후 (관례)
    {
        pauseButton.onClick.AddListener(TogglePause);
    }

    private void Update()
    {
        var skillManager = gameManager.SkillManager;
        var waveManager = gameManager.WaveManager;
        if (skillManager == null || skillManager.PlayerLevel == null) return;

        // 진행도 = 처치 누계 ÷ 전체 몬스터 수 (ResultPanel의 Fail 지표와 같은 정의)
        int total = waveManager.TotalMonsterCount;
        int percent = total > 0 ? skillManager.PlayerLevel.TotalKills * 100 / total : 0;
        if (percent != lastProgressPercent)
        {
            lastProgressPercent = percent;
            progressTarget = percent / 100f;
            progressText.text = $"{percent}%";
        }

        // 레벨 게이지 = 다음 레벨까지의 킬 진행 [가정A]
        PlayerLevel level = skillManager.PlayerLevel;
        if (level.Level != lastLevel || level.KillsIntoLevel != lastKillsIntoLevel)
        {
            // 첫 동기화(lastLevel == -1)는 레벨업이 아님 — 연출 없이 값만 맞춤 (시작 시 꽉 차던 버그)
            if (level.Level != lastLevel && lastLevel != -1)
                fillingToFull = true;   // 레벨업: 먼저 "꽉" 채우는 연출 — 리셋은 가득 찬 뒤 (원작 시퀀스)

            lastLevel = level.Level;
            lastKillsIntoLevel = level.KillsIntoLevel;
            levelText.text = $"Lv.{level.Level}";
        }
        levelTarget = fillingToFull ? 1f : (float)level.KillsIntoLevel / level.KillsToNext;

        // 부드럽게 차오르는 연출 (원작) — 목표값은 즉시, 표시는 보간
        progressSlider.value = Mathf.MoveTowards(progressSlider.value, progressTarget, gaugeFillSpeed * Time.deltaTime);
        levelSlider.value = Mathf.MoveTowards(levelSlider.value, levelTarget, gaugeFillSpeed * Time.deltaTime);

        // 꽉 찼으면 0으로 스냅 후 이월분부터 다시 참 — 스킬 선택(정지) 동안엔 가득 찬 채 유지되고,
        // 선택을 마치고 재개된 첫 프레임에 리셋된다 ("선택하면 0으로" — 유저 확정)
        if (fillingToFull && levelSlider.value >= 0.999f &&
            gameManager.CurrentState == GameManager.GameState.GamePlay)
        {
            fillingToFull = false;
            levelSlider.value = 0f;
        }
    }

    private void TogglePause()
    {
        // GamePlay에서만 진입 — 스킬 선택/결과 중 이중 정지 방지
        if (gameManager.CurrentState == GameManager.GameState.GamePlay)
            gameManager.SetGameState(GameManager.GameState.GameStop);
    }
}
