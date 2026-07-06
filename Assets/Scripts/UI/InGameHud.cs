using UnityEngine;
using UnityEngine.UI;

// 상단 HUD 밴드 — 스테이지명 · 진행도 바 · 레벨(킬) 게이지 · 일시정지 버튼 (원작 상단 구성, 배속/Auto는 제외).
// 항시 표시라 UIElement(Show/Hide 대상)가 아닌 일반 컴포넌트. 씬 관례: 창구 참조는 GameManager 하나.
// 갱신은 폴링 + 변경 감지 캐시 — 표시 전용이라 구독 수명 관리보다 단순하고, 값이 바뀔 때만 텍스트 할당(GC 회피).
public class InGameHud : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private string stageName = "1. 버려진 숲";   // 기획 수정 영역 [가정 — 원작식 "N. 이름" 표기]

    private Image progressFill;
    private Text progressText;
    private Image levelFill;
    private Text levelText;

    // 변경 감지 캐시 — 같은 값이면 UI 재할당 없음
    private int lastProgressPercent = -1;
    private int lastLevel = -1;
    private int lastKillsIntoLevel = -1;

    private void Start()   // 매니저 Initialize 완료 후 데이터 접근 (PlayerHpBar와 동일한 이유)
    {
        Build();
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

        // 레벨 게이지 = 다음 레벨까지의 킬 진행 [가정①]
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

    // 좌표는 전부 부모(TopBar 존, 높이 160) 상단 기준 — 존이 세이프 에어리어 안에서 잘리지 않게 보장
    private void Build()
    {
        // 스테이지명 (상단 중앙)
        Text stage = CreateText("StageName", 34, FontStyle.Bold, Color.white);
        Anchor(stage.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(500f, 44f));
        stage.text = stageName;

        // 진행도 바 — 원작: 검정 트랙 + 빨강 fill + %. 밝은 테두리로 배경과 분리
        Image progressBorder = CreateImage("ProgressBorder", new Color(0.85f, 0.8f, 0.7f, 0.9f));
        Anchor(progressBorder.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -66f), new Vector2(408f, 30f));
        Image progressTrack = CreateImage("ProgressTrack", new Color(0.08f, 0.08f, 0.08f, 0.95f));
        Anchor(progressTrack.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -66f), new Vector2(400f, 22f));
        progressFill = CreateFill(progressTrack, new Color(0.85f, 0.22f, 0.18f));
        progressText = CreateText("ProgressText", 20, FontStyle.Bold, Color.white);
        Anchor(progressText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -66f), new Vector2(400f, 26f));

        // 레벨(킬) 게이지 — 오른쪽에 배지(마름모)+Lv.N 텍스트 자리를 비워둠
        Image levelTrack = CreateImage("LevelTrack", new Color(0.1f, 0.09f, 0.07f, 0.9f));
        var levelRect = levelTrack.rectTransform;
        levelRect.anchorMin = new Vector2(0f, 1f);
        levelRect.anchorMax = new Vector2(1f, 1f);
        levelRect.offsetMin = new Vector2(40f, -122f);
        levelRect.offsetMax = new Vector2(-150f, -100f);
        levelFill = CreateFill(levelTrack, new Color(0.95f, 0.6f, 0.15f));

        // 배지: 흰 사각형을 45° 돌린 마름모 (전용 스프라이트 없이 원작 배지 자리 표현)
        Image badge = CreateImage("LevelBadge", new Color(0.95f, 0.6f, 0.15f));
        Anchor(badge.rectTransform, new Vector2(1f, 1f), new Vector2(-128f, -111f), new Vector2(26f, 26f));
        badge.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        levelText = CreateText("LevelText", 26, FontStyle.Bold, new Color(1f, 0.9f, 0.6f));
        Anchor(levelText.rectTransform, new Vector2(1f, 1f), new Vector2(-70f, -111f), new Vector2(100f, 40f));

        // 일시정지 버튼 (우상단)
        Image pauseBg = CreateImage("PauseButton", new Color(0.15f, 0.16f, 0.2f, 0.9f));
        Anchor(pauseBg.rectTransform, new Vector2(1f, 1f), new Vector2(-40f, -36f), new Vector2(58f, 58f));
        pauseBg.gameObject.AddComponent<Button>().onClick.AddListener(TogglePause);
        Text pauseGlyph = CreateText("PauseGlyph", 28, FontStyle.Bold, Color.white);
        Anchor(pauseGlyph.rectTransform, new Vector2(1f, 1f), new Vector2(-40f, -36f), new Vector2(58f, 58f));
        pauseGlyph.text = "II";
        pauseGlyph.raycastTarget = false;   // 버튼 클릭 방해 금지
    }

    private Image CreateImage(string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(transform, false);
        var image = go.AddComponent<Image>();
        image.sprite = RuntimeSprites.White;
        image.color = color;
        return image;
    }

    // 트랙 안쪽을 좌→우로 채우는 fill (Image Filled 타입 — 스케일 조작 없이 비율만)
    private Image CreateFill(Image track, Color color)
    {
        var go = new GameObject("Fill", typeof(RectTransform));
        go.transform.SetParent(track.transform, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(2f, 2f);
        rect.offsetMax = new Vector2(-2f, -2f);

        var image = go.AddComponent<Image>();
        image.sprite = RuntimeSprites.White;
        image.color = color;
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Horizontal;
        image.fillAmount = 0f;
        image.raycastTarget = false;
        return image;
    }

    private Text CreateText(string name, int fontSize, FontStyle style, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(transform, false);
        var text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        return text;
    }

    private static void Anchor(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }
}
