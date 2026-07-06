using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// 인게임 UI 1회 생성 도구 — 에디터 API로 조립하므로 직렬화가 항상 올바르고,
// 결과물은 순수 씬 오브젝트(Inspector 튜닝 가능). 실행 후 씬 저장하면 끝, 빌드엔 포함 안 됨.
// 손 YAML 조립은 Slider/TMP 같은 복잡 컴포넌트에서 파서 불일치 사고가 나서 이 방식으로 확정 (2026-07-06).
public static class InGameUIBuilder
{
    private static TMP_FontAsset font;

    [MenuItem("Tools/Build InGame UI (1회 실행)")]
    public static void Build()
    {
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Font/Kostar SDF 2.asset");
        if (font == null) { Debug.LogError("Kostar SDF 2.asset 없음 — Assets/Font 확인"); return; }

        var topBar = GameObject.Find("TopBar")?.GetComponent<RectTransform>();
        var bottomBar = GameObject.Find("BottomBar")?.GetComponent<RectTransform>();
        var hud = Object.FindFirstObjectByType<InGameHud>(FindObjectsInactive.Include);
        var pause = Object.FindFirstObjectByType<PausePanel>(FindObjectsInactive.Include);
        var result = Object.FindFirstObjectByType<ResultPanel>(FindObjectsInactive.Include);
        var cards = Object.FindFirstObjectByType<SkillSelectionPanel>(FindObjectsInactive.Include);
        var hpBar = Object.FindFirstObjectByType<PlayerHpBar>(FindObjectsInactive.Include);
        if (topBar == null || hud == null || pause == null || result == null || cards == null)
        { Debug.LogError("씬 구조가 예상과 다름 (TopBar/패널들 확인)"); return; }

        BuildHud(hud, topBar);
        BuildPause(pause);
        BuildResult(result);
        BuildCards(cards);
        BuildHpSlider(hpBar, bottomBar);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[InGameUIBuilder] 완료 — 씬 저장(Cmd+S)하면 확정됩니다");
    }

    // ── HUD (TopBar 안) ──────────────────────────────────────────
    private static void BuildHud(InGameHud hud, RectTransform topBar)
    {
        Clear(hud.transform);
        hud.transform.SetParent(topBar, false);
        Stretch((RectTransform)hud.transform);

        Text(hud.transform, "StageName", "1. 깊은 숲", 34, new(0.5f, 1f), new(0, -28), new(500, 44), bold: true);

        Image(hud.transform, "ProgressBorder", new Color(0.85f, 0.8f, 0.7f, 0.9f), new(0.5f, 1f), new(0, -66), new(408, 30));
        var track = Image(hud.transform, "ProgressTrack", new Color(0.08f, 0.08f, 0.08f, 0.95f), new(0.5f, 1f), new(0, -66), new(400, 22));
        var pFill = Fill(track.transform, new Color(0.85f, 0.22f, 0.18f));
        var pText = Text(hud.transform, "ProgressText", "0%", 20, new(0.5f, 1f), new(0, -66), new(400, 26), bold: true);

        var lTrack = Image(hud.transform, "LevelTrack", new Color(0.1f, 0.09f, 0.07f, 0.9f), new(0.5f, 1f), new(0, -111), new(400, 22));
        var lRect = lTrack.rectTransform;
        lRect.anchorMin = new(0, 1); lRect.anchorMax = new(1, 1);
        lRect.offsetMin = new(40, -122); lRect.offsetMax = new(-150, -100);
        var lFill = Fill(lTrack.transform, new Color(0.95f, 0.6f, 0.15f));
        var badge = Image(hud.transform, "LevelBadge", new Color(0.95f, 0.6f, 0.15f), new(1, 1), new(-128, -111), new(26, 26));
        badge.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
        var lText = Text(hud.transform, "LevelText", "Lv.1", 26, new(1, 1), new(-70, -111), new(100, 40), bold: true, color: new Color(1f, 0.9f, 0.6f));

        var pauseBtn = ButtonBox(hud.transform, "PauseButton", "II", 28, new Color(0.15f, 0.16f, 0.2f, 0.9f), new(1, 1), new(-40, -36), new(58, 58));

        Assign(hud, ("progressFill", pFill), ("progressText", pText), ("levelFill", lFill),
                    ("levelText", lText), ("pauseButton", pauseBtn));
    }

    // ── 일시정지 (원작 #36 비율) ─────────────────────────────────
    private static void BuildPause(PausePanel pause)
    {
        Clear(pause.transform);
        var overlay = Overlay(pause.transform, 0.9f);

        Text(overlay, "Title", "일시정지", 76, F(0.87f), Vector2.zero, new(640, 90), color: new Color(1f, 0.9f, 0.6f));
        Text(overlay, "Stage", "Stage 1  (Normal)", 34, F(0.78f), Vector2.zero, new(640, 60), color: new Color(0.6f, 0.8f, 1f));
        var combat = ButtonBox(overlay, "CombatInfoButton", "il", 26, new Color(0.3f, 0.32f, 0.38f), F(0.78f), new(240, 0), new(52, 52));

        Text(overlay, "ActiveLabel", "Active Skill", 28, F(0.665f), new(-250, 0), new(400, 40), color: new Color(1f, 0.6f, 0.55f));
        Text(overlay, "PassiveLabel", "Passive Skill", 28, F(0.665f), new(275, 0), new(400, 40), color: new Color(0.55f, 0.9f, 0.85f));

        var active = new Image[4]; var passive = new Image[2];
        for (int i = 0; i < 4; i++) active[i] = Slot(overlay, $"ActiveSlot{i}", new Color(0.55f, 0.2f, 0.18f), new(-430 + i * 128, 0));
        for (int i = 0; i < 2; i++) passive[i] = Slot(overlay, $"PassiveSlot{i}", new Color(0.16f, 0.45f, 0.42f), new(160 + i * 128, 0));

        Text(overlay, "DropLabel", "+ 현재 스테이지 드랍", 28, F(0.50f), new(-250, 0), new(500, 40), color: new Color(0.75f, 0.75f, 0.9f));
        Image(overlay, "DropFrame", new Color(0.09f, 0.09f, 0.12f, 0.95f), F(0.35f), Vector2.zero, new(920, 440));

        var resume = ButtonBox(overlay, "ResumeButton", "이어하기", 40, new Color(0.95f, 0.65f, 0.2f), F(0.12f), Vector2.zero, new(380, 100), boldLabel: true);

        Assign(pause, ("overlay", overlay.gameObject), ("resumeButton", resume), ("combatInfoButton", combat));
        AssignArray(pause, "activeIcons", active);
        AssignArray(pause, "passiveIcons", passive);
        overlay.gameObject.SetActive(false);
    }

    // ── 결과 ─────────────────────────────────────────────────────
    private static void BuildResult(ResultPanel result)
    {
        Clear(result.transform);
        var overlay = Overlay(result.transform, 0.75f);
        var panel = Image(overlay, "Panel", new Color(0.13f, 0.12f, 0.16f, 0.97f), C, Vector2.zero, new(520, 480)).transform;

        var title = Text(panel, "Title", "Stage Fail", 60, C, new(0, 140), new(480, 80), color: new Color(1f, 0.85f, 0.4f));
        Text(panel, "Stage", "Stage 1  (Normal)", 30, C, new(0, 60), new(480, 50), color: new Color(0.6f, 0.8f, 1f));
        var info = Text(panel, "Info", "", 34, C, new(0, -10), new(480, 60));
        var restart = ButtonBox(panel, "RestartButton", "다시 시작", 36, new Color(0.95f, 0.65f, 0.2f), C, new(0, -150), new(300, 88), boldLabel: true);

        Assign(result, ("overlay", overlay.gameObject), ("titleText", title), ("infoText", info), ("restartButton", restart));
        overlay.gameObject.SetActive(false);
    }

    // ── 3택지 카드 ───────────────────────────────────────────────
    private static void BuildCards(SkillSelectionPanel panel)
    {
        Clear(panel.transform);
        var overlay = Overlay(panel.transform, 0.6f);

        var buttons = new Button[3]; var icons = new Image[3];
        var names = new TMP_Text[3]; var levels = new TMP_Text[3]; var descs = new TMP_Text[3];
        for (int i = 0; i < 3; i++)
        {
            var card = Image(overlay, $"Card{i}", new Color(0.16f, 0.22f, 0.2f, 0.95f), C, new((i - 1) * 350, 0), new(320, 460));
            card.raycastTarget = true;
            buttons[i] = card.gameObject.AddComponent<Button>();
            buttons[i].targetGraphic = card;

            icons[i] = Image(card.transform, "Icon", Color.white, C, new(0, 110), new(150, 150), sprite: false);
            icons[i].preserveAspect = true;
            names[i] = Text(card.transform, "Name", "", 38, C, new(0, -10), new(300, 50), bold: true);
            levels[i] = Text(card.transform, "Level", "", 26, C, new(0, -55), new(300, 36), color: new Color(1f, 0.85f, 0.4f));
            descs[i] = Text(card.transform, "Description", "", 24, C, new(0, -140), new(280, 120), color: new Color(0.85f, 0.88f, 0.85f));
        }

        Assign(panel, ("overlay", overlay.gameObject));
        AssignArray(panel, "buttons", buttons);
        AssignArray(panel, "icons", icons);
        AssignArray(panel, "names", names);
        AssignArray(panel, "levels", levels);
        AssignArray(panel, "descriptions", descs);
        overlay.gameObject.SetActive(false);
    }

    // ── 플레이어 HP Slider (BottomBar) ───────────────────────────
    private static void BuildHpSlider(PlayerHpBar hpBar, RectTransform bottomBar)
    {
        foreach (Transform old in bottomBar) if (old.name == "PlayerHpSlider") { Object.DestroyImmediate(old.gameObject); break; }

        var root = Child(bottomBar, "PlayerHpSlider", C, new(0, 15), new(380, 36));
        var slider = root.gameObject.AddComponent<Slider>();
        var bg = Image(root, "Background", new Color(0.08f, 0.08f, 0.08f, 0.9f), C, Vector2.zero, Vector2.zero);
        Stretch(bg.rectTransform);

        var fillArea = Child(root, "Fill Area", C, Vector2.zero, new(-8, -8));
        Stretch(fillArea); fillArea.offsetMin = new(4, 4); fillArea.offsetMax = new(-4, -4);
        var fill = Image(fillArea, "Fill", new Color(0.35f, 0.9f, 0.3f), C, Vector2.zero, new(10, 0));
        fill.rectTransform.anchorMin = new(0, 0); fill.rectTransform.anchorMax = new(0, 1);

        slider.fillRect = fill.rectTransform;
        slider.direction = Slider.Direction.LeftToRight;
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;
        slider.minValue = 0; slider.maxValue = 1; slider.value = 1;

        var text = Text(root, "HpText", "300", 24, C, Vector2.zero, Vector2.zero, bold: true);
        Stretch(text.rectTransform);

        Assign(hpBar, ("hpSlider", slider), ("hpText", text));
    }

    // ── 조립 헬퍼 ────────────────────────────────────────────────
    private static readonly Vector2 C = new(0.5f, 0.5f);
    private static Vector2 F(float y) => new(0.5f, y);

    private static void Clear(Transform t)
    {
        for (int i = t.childCount - 1; i >= 0; i--) Object.DestroyImmediate(t.GetChild(i).gameObject);
    }

    private static void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    private static RectTransform Child(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = 5;
        var r = (RectTransform)go.transform;
        r.SetParent(parent, false);
        r.anchorMin = r.anchorMax = anchor;
        r.anchoredPosition = pos;
        r.sizeDelta = size;
        return r;
    }

    private static RectTransform Overlay(Transform parent, float dim)
    {
        var r = Child(parent, "Overlay", C, Vector2.zero, Vector2.zero);
        Stretch(r);
        var img = r.gameObject.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0, 0, 0, dim);
        img.raycastTarget = true;
        return r;
    }

    private static Image Image(Transform parent, string name, Color color, Vector2 anchor, Vector2 pos, Vector2 size, bool sprite = true)
    {
        var r = Child(parent, name, anchor, pos, size);
        var img = r.gameObject.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        if (sprite)
        {
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = UnityEngine.UI.Image.Type.Sliced;
        }
        return img;
    }

    private static Image Fill(Transform track, Color color)
    {
        var img = Image(track, "Fill", color, C, Vector2.zero, new(-4, -4));
        Stretch(img.rectTransform);
        img.rectTransform.offsetMin = new(2, 2); img.rectTransform.offsetMax = new(-2, -2);
        img.type = UnityEngine.UI.Image.Type.Filled;
        img.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
        img.fillAmount = 0f;
        return img;
    }

    private static Image Slot(Transform parent, string name, Color frame, Vector2 pos)
    {
        var frameImg = Image(parent, name, frame, F(0.60f), pos, new(116, 116));
        var icon = Image(frameImg.transform, "Icon", new Color(0.05f, 0.05f, 0.06f), C, Vector2.zero, new(104, 104), sprite: false);
        return icon;
    }

    private static TMP_Text Text(Transform parent, string name, string content, int size, Vector2 anchor, Vector2 pos, Vector2 rect,
                                 bool bold = false, Color? color = null)
    {
        var r = Child(parent, name, anchor, pos, rect);
        var t = r.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = font;
        t.text = content;
        t.fontSize = size;
        t.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        t.alignment = TextAlignmentOptions.Center;
        t.color = color ?? Color.white;
        t.raycastTarget = false;
        return t;
    }

    private static Button ButtonBox(Transform parent, string name, string label, int labelSize, Color bg,
                                    Vector2 anchor, Vector2 pos, Vector2 size, bool boldLabel = false)
    {
        var img = Image(parent, name, bg, anchor, pos, size);
        img.raycastTarget = true;
        var btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        var t = Text(img.transform, "Label", label, labelSize, C, Vector2.zero, size, bold: boldLabel);
        return btn;
    }

    // SerializedObject로 private [SerializeField]에 참조 할당 (에디터 전용 정석)
    private static void Assign(Object target, params (string field, Object value)[] refs)
    {
        var so = new SerializedObject(target);
        foreach (var (field, value) in refs)
        {
            var prop = so.FindProperty(field);
            if (prop == null) { Debug.LogWarning($"{target.name}: 필드 {field} 없음"); continue; }
            prop.objectReferenceValue = value;
        }
        so.ApplyModifiedProperties();
    }

    private static void AssignArray<T>(Object target, string field, T[] values) where T : Object
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(field);
        if (prop == null) { Debug.LogWarning($"{target.name}: 필드 {field} 없음"); return; }
        prop.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedProperties();
    }
}
