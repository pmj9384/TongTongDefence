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
        if (gameManager.CurrentState == GameManager.GameState.GameClear)
        {
            PlayerHealth health = gameManager.PlayerManager.Health;
            int percent = health.Current * 100 / health.Max;
            StartCoroutine(ShowAfter(clearDelay, "Stage Clear!", $"남은 체력 {percent}%"));
        }
        else
        {
            // 진행도 = 처치 누계 ÷ 전체 몬스터 수 (InGameHud와 같은 정의)
            int total = gameManager.WaveManager.TotalMonsterCount;
            int percent = total > 0 ? gameManager.SkillManager.PlayerLevel.TotalKills * 100 / total : 0;
            StartCoroutine(ShowAfter(failDelay, "Stage Fail", $"진행도 {percent}%"));
        }
    }

    public override void Hide() => overlay.SetActive(false);

    private IEnumerator ShowAfter(float delay, string title, string info)
    {
        // 결과 상태는 timeScale 0 (게임 정지) — 스케일 시간을 쓰면 이 딜레이가 영원히 안 끝난다
        yield return new WaitForSecondsRealtime(delay);

        titleText.text = title;
        infoText.text = info;
        overlay.SetActive(true);
    }
}
