using UnityEngine;
using TMPro;
using UnityEngine.UI;

// 상단 HUD — 값 갱신 로직만. 배치/색/크기는 전부 씬(Inspector) 소관 (에디터-네이티브 전환, 2026-07-06).
// 갱신은 폴링 + 변경 감지 캐시 — 값이 바뀐 프레임에만 텍스트 할당 (GC 회피).
public class InGameHud : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    [Header("씬 참조 (TopBar 아래 조립됨)")]
    [SerializeField] private Image progressFill;    // 진행도 빨강 fill (Filled/Horizontal)
    [SerializeField] private TMP_Text progressText;     // "N%"
    [SerializeField] private Image levelFill;       // 레벨 게이지 주황 fill
    [SerializeField] private TMP_Text levelText;        // "Lv.N"
    [SerializeField] private Button pauseButton;

    private int lastProgressPercent = -1;
    private int lastLevel = -1;
    private int lastKillsIntoLevel = -1;

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
            progressFill.fillAmount = percent / 100f;
            progressText.text = $"{percent}%";
        }

        // 레벨 게이지 = 다음 레벨까지의 킬 진행 [가정A]
        PlayerLevel level = skillManager.PlayerLevel;
        if (level.Level != lastLevel || level.KillsIntoLevel != lastKillsIntoLevel)
        {
            lastLevel = level.Level;
            lastKillsIntoLevel = level.KillsIntoLevel;
            levelFill.fillAmount = (float)level.KillsIntoLevel / level.KillsToNext;
            levelText.text = $"Lv.{level.Level}";
        }
    }

    private void TogglePause()
    {
        // GamePlay에서만 진입 — 스킬 선택/결과 중 이중 정지 방지
        if (gameManager.CurrentState == GameManager.GameState.GamePlay)
            gameManager.SetGameState(GameManager.GameState.GameStop);
    }
}
