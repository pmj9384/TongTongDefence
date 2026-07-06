using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 결과 팝업 (기획서 필수 5항) — 성공/실패 배너 + 지표 + [다시 시작].
// 템플릿 UIElement 체계 소속: GameUIManager가 GameOver/GameClear 진입 시 Show() 호출,
// 어느 결과인지는 CurrentState로 판별 (SetGameState가 상태 할당 후 액션을 돌리므로 안전).
// 원작 관찰: Clear는 "남은 체력 %", Fail은 "진행도 %"를 같은 자리에 표시.
// 별점/보상 그리드/광고 부활은 메타 경제라 범위 외 (README 언급).
public class ResultPanel : UIElement
{
    [SerializeField] private float failDelay = 1f;      // 죽음 연출(쓰러짐) 뜸
    [SerializeField] private float clearDelay = 0.3f;

    private GameObject overlay;
    private Text titleText;
    private Text infoText;

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
            // 진행도 = 처치 누계 ÷ 전체 몬스터 수 (도달 소멸은 킬이 아니라 자동 제외 — 원작 "처치 비율"과 일치)
            int total = gameManager.WaveManager.TotalMonsterCount;
            int percent = total > 0 ? gameManager.SkillManager.PlayerLevel.TotalKills * 100 / total : 0;
            StartCoroutine(ShowAfter(failDelay, "Stage Fail", $"진행도 {percent}%"));
        }
    }

    public override void Hide()
    {
        if (overlay != null) overlay.SetActive(false);
    }

    private IEnumerator ShowAfter(float delay, string title, string info)
    {
        // 결과 상태는 timeScale 0 (게임 정지) — 스케일 시간을 쓰면 이 딜레이가 영원히 안 끝난다
        yield return new WaitForSecondsRealtime(delay);

        if (overlay == null) Build();
        titleText.text = title;
        infoText.text = info;
        overlay.SetActive(true);
    }

    private void Restart() => gameManager.RestartGame();

    // 어두운 오버레이 + 중앙 패널 (1회 생성)
    private void Build()
    {
        overlay = CreateChild("Overlay", transform);
        Image dim = overlay.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.75f);
        Stretch(overlay.GetComponent<RectTransform>());

        GameObject panel = CreateChild("Panel", overlay.transform);
        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0.13f, 0.12f, 0.16f, 0.97f);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(520f, 480f);

        titleText = CreateText(panel.transform, "Title", 60, new Vector2(0f, 140f), new Color(1f, 0.85f, 0.4f));
        CreateText(panel.transform, "Stage", 30, new Vector2(0f, 60f), new Color(0.6f, 0.8f, 1f)).text = "Stage 1  (Normal)";
        infoText = CreateText(panel.transform, "Info", 34, new Vector2(0f, -10f), Color.white);

        // [다시 시작] 버튼
        GameObject buttonGo = CreateChild("RestartButton", panel.transform);
        Image buttonBg = buttonGo.AddComponent<Image>();
        buttonBg.color = new Color(0.95f, 0.65f, 0.2f);
        var buttonRect = buttonGo.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(300f, 88f);
        buttonRect.anchoredPosition = new Vector2(0f, -150f);
        buttonGo.AddComponent<Button>().onClick.AddListener(Restart);

        Text buttonLabel = CreateText(buttonGo.transform, "Label", 36, Vector2.zero, Color.white);
        buttonLabel.text = "다시 시작";
    }

    private static GameObject CreateChild(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Text CreateText(Transform parent, string name, int fontSize, Vector2 position, Color color)
    {
        GameObject go = CreateChild(name, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(480f, 80f);
        rect.anchoredPosition = position;

        var text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        return text;
    }
}
