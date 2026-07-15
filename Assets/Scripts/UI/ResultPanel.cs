using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

// 결과 팝업 (기획서 필수 5항, UIElement) — 지표 계산/딜레이 로직만. 배치는 씬 소관 (에디터-네이티브).
// 원작 관찰: Clear는 "남은 체력 %", Fail은 "진행도 %"를 같은 자리에 표시.
public class ResultPanel : UIElement
{
    [SerializeField] private float failDelay = 1f;      // 죽음 연출(쓰러짐) 뜸
    [SerializeField] private float clearDelay = 0.3f;

    [Header("씬 참조")]
    [SerializeField] private GameObject overlay;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private Button restartButton;

    private void Awake()
    {
        restartButton.onClick.AddListener(() => gameManager.RestartGame());
    }

    public override void Show()
    {
        bool clear = gameManager.CurrentState == GameManager.GameState.GameClear;
        StartCoroutine(ShowAfter(clear ? clearDelay : failDelay));
    }

    public override void Hide() => overlay.SetActive(false);

    // 지표는 딜레이가 끝난 뒤 읽는다 — GameOver 진입 액션(StatsManager.SubmitScore 등)이 모두 끝난 시점이라
    // 최고기록 갱신 순서에 안전. (결과 상태는 timeScale 0이라 반드시 Realtime 딜레이)
    private IEnumerator ShowAfter(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (gameManager.CurrentState == GameManager.GameState.GameClear)
        {
            PlayerHealth health = gameManager.PlayerManager.Health;
            int percent = health.Current * 100 / health.Max;
            titleText.text = "Stage Clear!";
            infoText.text = $"남은 체력 {percent}%";
        }
        else
        {
            // 무한모드: 진행도% 대신 이번 점수 + 최고(+신기록). 점수 = StatsManager SSOT
            StatsManager stats = gameManager.StatsManager;
            titleText.text = "Game Over";
            infoText.text = stats.IsNewRecord
                ? $"SCORE {stats.Score.Current:N0}   신기록!\n최고 {stats.Best:N0}"
                : $"SCORE {stats.Score.Current:N0}\n최고 {stats.Best:N0}";
        }
        overlay.SetActive(true);
    }
}
