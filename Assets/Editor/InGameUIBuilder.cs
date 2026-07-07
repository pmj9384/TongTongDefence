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

    // 조준선을 점선+끝점 조준점으로 (원작 관찰) — 기존 파츠 눈튜닝을 건드리지 않는 별도 메뉴
    [MenuItem("Tools/Build Aim Line (점선+조준점)")]
    public static void BuildAimLine()
    {
        var shooter = Object.FindFirstObjectByType<Shooter>(FindObjectsInactive.Include);
        if (shooter == null) { Debug.LogError("Shooter 없음"); return; }

        // 점선 머티리얼 (dash 텍스처 + Tile 모드)
        var dashTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Sprites/UI/aim_dash.png");
        if (dashTex == null) { Debug.LogError("aim_dash.png 없음"); return; }
        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/AimDashed.mat");
        if (mat == null)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Materials")) AssetDatabase.CreateFolder("Assets", "Materials");
            mat = new Material(Shader.Find("Sprites/Default")) { mainTexture = dashTex };
            AssetDatabase.CreateAsset(mat, "Assets/Materials/AimDashed.mat");
        }
        mat.mainTexture = dashTex;
        mat.mainTextureScale = new Vector2(14f, 1f);   // 반복 밀도 ↑ = 짧은 점이 촘촘히 (값 키울수록 촘촘)
        EditorUtility.SetDirty(mat);

        var line = shooter.GetComponent<LineRenderer>();
        line.sharedMaterial = mat;
        line.textureMode = LineTextureMode.Tile;   // 길이 따라 dash 반복 = 점선
        line.startColor = line.endColor = new Color(0.72f, 0.72f, 0.7f, 0.9f);    // 원작: 회색
        line.startWidth = line.endWidth = 0.045f;                                  // 가는 선 (원작 #52)

        // 끝점 조준 레티클 (원작 #53: 끊긴 링 + 빨간 중심점) — 재실행 시 스프라이트 갱신
        Transform dot = shooter.transform.Find("AimDot");
        if (dot == null)
        {
            var dotGo = new GameObject("AimDot");
            dot = dotGo.transform;
            dot.SetParent(shooter.transform, false);
        }
        dot.localScale = Vector3.one * 1.2f;
        var reticleSr = dot.GetComponent<SpriteRenderer>() ?? dot.gameObject.AddComponent<SpriteRenderer>();
        reticleSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/UI/aim_reticle.png");
        reticleSr.color = Color.white;
        reticleSr.sortingOrder = 5;
        var so = new SerializedObject(shooter);
        so.FindProperty("aimDot").objectReferenceValue = dot;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[InGameUIBuilder] 점선 조준선 + 조준점 완료 — 씬 저장하세요");
    }

    // 전투 정보 창 조립 (원작 #57) — SafeAreaPanel 아래 패널 생성 + 행 7개 + 참조 자동 할당
    [MenuItem("Tools/Build CombatInfo Panel")]
    public static void BuildCombatInfoPanel()
    {
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Font/Kostar SDF 2.asset");
        var safeArea = GameObject.Find("SafeAreaPanel")?.GetComponent<RectTransform>();
        if (safeArea == null || font == null) { Debug.LogError("SafeAreaPanel/폰트 확인"); return; }

        // 패널 루트 (재실행 시 기존 것 제거 후 재조립)
        var oldPanel = Object.FindFirstObjectByType<CombatInfoPanel>(FindObjectsInactive.Include);
        if (oldPanel != null) Object.DestroyImmediate(oldPanel.gameObject);
        var root = Child(safeArea, "CombatInfoPanel", C, Vector2.zero, Vector2.zero);
        Stretch(root);
        var panel = root.gameObject.AddComponent<CombatInfoPanel>();

        var overlay = Overlay(root, 0.92f);
        var closeBtn = overlay.gameObject.AddComponent<Button>();   // 전체 터치 = 닫기
        closeBtn.targetGraphic = overlay.GetComponent<UnityEngine.UI.Image>();
        closeBtn.transition = Selectable.Transition.None;

        Text(overlay, "Title", "전투 정보", 56, F(0.90f), Vector2.zero, new(640, 80), color: new Color(1f, 0.85f, 0.5f));
        Text(overlay, "Stage", "Stage 1  (Normal)", 32, F(0.83f), Vector2.zero, new(640, 50), color: new Color(0.6f, 0.8f, 1f));
        var pSlider = SliderGauge(overlay, "ProgressSlider", new Color(0.85f, 0.22f, 0.18f), F(0.78f), Vector2.zero, new(420, 20));
        var pText = Text(overlay, "ProgressText", "0%", 20, F(0.78f), Vector2.zero, new(420, 26), bold: true);
        Text(overlay, "CloseHint", "터치하여 닫기", 26, F(0.05f), Vector2.zero, new(400, 40), color: new Color(0.6f, 0.6f, 0.6f));

        const int RowCount = 7;
        var rows = new GameObject[RowCount];
        var icons = new UnityEngine.UI.Image[RowCount];
        var levels = new TMP_Text[RowCount];
        var names = new TMP_Text[RowCount];
        var totals = new TMP_Text[RowCount];
        var ratios = new Slider[RowCount];
        var dpss = new TMP_Text[RowCount];
        for (int i = 0; i < RowCount; i++)
        {
            float y = 0.68f - i * 0.085f;   // 위에서부터 행 배치
            var row = Child(overlay, $"Row{i}", F(y), Vector2.zero, new(900, 90));
            rows[i] = row.gameObject;

            icons[i] = Image(row, "Icon", Color.white, C, new(-380, 8), new(80, 80), sprite: false);
            icons[i].preserveAspect = true;
            levels[i] = Text(row, "Level", "◆x1", 26, C, new(-380, -38), new(130, 32), color: new Color(1f, 0.85f, 0.4f));
            names[i] = Text(row, "Name", "", 34, C, new(-88, 26), new(420, 44), bold: true);   // x=-88: 슬라이더 왼끝 정렬 (유저 눈튜닝)
            names[i].horizontalAlignment = TMPro.HorizontalAlignmentOptions.Left;   // 원작: 아이콘 옆 좌측 정렬
            totals[i] = Text(row, "Total", "0", 38, C, new(-88, -14), new(420, 48), bold: true, color: new Color(0.95f, 0.9f, 0.7f));
            totals[i].horizontalAlignment = TMPro.HorizontalAlignmentOptions.Left;
            ratios[i] = SliderGauge(row, "Ratio", new Color(0.85f, 0.22f, 0.18f), C, new(-70, -44), new(460, 24));   // 원작 두께
            Text(row, "DpsLabel", "DPS", 30, C, new(330, 26), new(150, 38), bold: true, color: new Color(0.9f, 0.88f, 0.8f));
            dpss[i] = Text(row, "Dps", "0", 36, C, new(330, -14), new(150, 44), bold: true);
        }

        Assign(panel, ("overlay", overlay.gameObject), ("closeButton", closeBtn),
                      ("progressText", pText), ("progressSlider", pSlider));
        AssignArray(panel, "rows", rows);
        AssignArray(panel, "icons", icons);
        AssignArray(panel, "levels", levels);
        AssignArray(panel, "names", names);
        AssignArray(panel, "totals", totals);
        AssignArray(panel, "ratios", ratios);
        AssignArray(panel, "dpsTexts", dpss);
        overlay.gameObject.SetActive(false);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[InGameUIBuilder] CombatInfoPanel 조립 완료 — Generate UIElement Enum 실행 후 씬 저장");
    }

    // 캐릭터 파츠 조립(조준 연출용) + PlayerHpBar 컴포넌트를 UI 오브젝트로 이사
    [MenuItem("Tools/Build Shooter Parts + Move HpBar")]
    public static void BuildShooterParts()
    {
        var shooter = Object.FindFirstObjectByType<Shooter>(FindObjectsInactive.Include);
        if (shooter == null) { Debug.LogError("Shooter 없음"); return; }
        Transform visual = shooter.transform.Find("Visual");
        if (visual == null) { Debug.LogError("Shooter/Visual 없음"); return; }

        // 통짜 스프라이트 제거 → 파츠 3장 (배치값은 시작점 — Inspector에서 눈튜닝)
        var oldSr = visual.GetComponent<SpriteRenderer>();
        if (oldSr != null) Object.DestroyImmediate(oldSr);
        for (int i = visual.childCount - 1; i >= 0; i--) Object.DestroyImmediate(visual.GetChild(i).gameObject);

        Sprite Load(string name) => AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/Sprites/Characters/{name}.png");
        SpriteRenderer Part(string name, Sprite sprite, Vector3 pos, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(visual, false);
            go.transform.localPosition = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            return sr;
        }

        var body = Part("Body", Load("Character_Main_body"), Vector3.zero, 1);

        // 머리: "목 위치"의 빈 피벗 기준 회전 — 자기 중심 회전이면 목에서 분리돼 보임
        var headPivot = new GameObject("HeadPivot").transform;
        headPivot.SetParent(visual, false);
        headPivot.localPosition = new Vector3(0f, 0.35f, 0f);   // 목 위치 [눈튜닝]
        var head = Part("Head", Load("Character_Main_head"), Vector3.zero, 2);
        head.transform.SetParent(headPivot, false);
        head.transform.localPosition = new Vector3(0f, 0.18f, 0f);   // 목→머리 중심 거리 [눈튜닝]

        var pivot = new GameObject("WeaponPivot").transform;
        pivot.SetParent(visual, false);
        pivot.localPosition = new Vector3(0.25f, 0.1f, 0f);   // 손잡이 위치 [눈튜닝]
        var weapon = Part("Weapon", Load("Character_main_weapon"), Vector3.zero, 0);   // 지팡이는 머리·몸 뒤 (원작)
        weapon.transform.SetParent(pivot, false);
        weapon.transform.localPosition = new Vector3(0f, 0.3f, 0f);   // 피벗에서 지팡이 몸통까지 [눈튜닝]

        var sv = visual.gameObject.GetComponent<ShooterVisual>();
        if (sv == null) sv = visual.gameObject.AddComponent<ShooterVisual>();
        var svSo = new SerializedObject(sv);
        svSo.FindProperty("body").objectReferenceValue = body;
        svSo.FindProperty("headPivot").objectReferenceValue = headPivot;
        svSo.FindProperty("weaponPivot").objectReferenceValue = pivot;
        svSo.ApplyModifiedProperties();

        var shooterSo = new SerializedObject(shooter);
        shooterSo.FindProperty("visual").objectReferenceValue = sv;
        shooterSo.ApplyModifiedProperties();

        // PlayerHpBar 이사: Shooter → PlayerHpSlider (직렬화 값 복사 후 원본 제거)
        var oldBar = shooter.GetComponent<PlayerHpBar>();
        var sliderGo = GameObject.Find("PlayerHpSlider");
        if (oldBar != null && sliderGo != null && sliderGo.GetComponent<PlayerHpBar>() == null)
        {
            var newBar = sliderGo.AddComponent<PlayerHpBar>();
            EditorUtility.CopySerialized(oldBar, newBar);
            Object.DestroyImmediate(oldBar);
            Debug.Log("[InGameUIBuilder] PlayerHpBar → PlayerHpSlider 이사 완료");
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[InGameUIBuilder] 캐릭터 파츠 조립 완료 — Head/WeaponPivot 위치를 Scene 뷰에서 눈튜닝 후 저장");
    }

    // 데미지 팝업 프리팹 생성 + 씬 MonsterManager에 연결 (TMP는 손 YAML 금지 규약 → 에디터 API로)
    [MenuItem("Tools/Build DamagePopup Prefab")]
    public static void BuildDamagePopupPrefab()
    {
        var kostar = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Font/Kostar SDF 2.asset");
        if (kostar == null) { Debug.LogError("Kostar SDF 2.asset 없음"); return; }

        var go = new GameObject("DamagePopup");
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.font = kostar;
        tmp.fontSize = 2.4f;                               // 월드 TMP 크기 — 2차 축소 (유저 확정 2026-07-07) [눈튜닝]
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.text = "99";
        tmp.GetComponent<MeshRenderer>().sortingOrder = 10;   // 몬스터/볼 위
        go.AddComponent<DamagePopup>();

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/UI/DamagePopup.prefab");
        Object.DestroyImmediate(go);

        var monsterManager = Object.FindFirstObjectByType<MonsterManager>(FindObjectsInactive.Include);
        if (monsterManager != null)
        {
            var so = new SerializedObject(monsterManager);
            so.FindProperty("damagePopupPrefab").objectReferenceValue = prefab;
            so.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
        Debug.Log("[InGameUIBuilder] DamagePopup.prefab 생성 + MonsterManager 연결 완료 — 씬 저장하세요");
    }

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

    // HUD만 재조립 (다른 패널의 눈튜닝 보존용 별도 메뉴)
    [MenuItem("Tools/Build HUD Only (게이지 Slider 통일)")]
    public static void BuildHudOnly()
    {
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Font/Kostar SDF 2.asset");
        var topBar = GameObject.Find("TopBar")?.GetComponent<RectTransform>();
        var hud = Object.FindFirstObjectByType<InGameHud>(FindObjectsInactive.Include);
        if (topBar == null || hud == null || font == null) { Debug.LogError("TopBar/InGameHud/폰트 확인"); return; }
        BuildHud(hud, topBar);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[InGameUIBuilder] HUD 재조립 완료 (Slider 게이지) — 씬 저장하세요");
    }

    // ── HUD (TopBar 안) — 게이지는 전부 UGUI Slider (플레이어 HP와 방식 통일) ──
    private static void BuildHud(InGameHud hud, RectTransform topBar)
    {
        Clear(hud.transform);
        hud.transform.SetParent(topBar, false);
        Stretch((RectTransform)hud.transform);

        Text(hud.transform, "StageName", "1. 깊은 숲", 34, new(0.5f, 1f), new(0, -28), new(500, 44), bold: true);

        Image(hud.transform, "ProgressBorder", new Color(0.85f, 0.8f, 0.7f, 0.9f), new(0.5f, 1f), new(0, -66), new(408, 30));
        var pSlider = SliderGauge(hud.transform, "ProgressSlider", new Color(0.85f, 0.22f, 0.18f),
                                  new(0.5f, 1f), new(0, -66), new(400, 22));
        var pText = Text(hud.transform, "ProgressText", "0%", 20, new(0.5f, 1f), new(0, -66), new(400, 26), bold: true);

        // 레벨 게이지: 중앙 고정폭 + 굵게 + 우측 끝 배지 (원작 #35 — 스트레치 앵커 폐기: 화면 폭 따라 길어지던 문제)
        var lSlider = SliderGauge(hud.transform, "LevelSlider", new Color(0.95f, 0.6f, 0.15f),
                                  new(0.5f, 1f), new(-35, -117), new(560, 34));
        var badge = Image(hud.transform, "LevelBadge", new Color(0.95f, 0.6f, 0.15f), new(0.5f, 1f), new(265, -117), new(34, 34));
        badge.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
        var lText = Text(hud.transform, "LevelText", "Lv.1", 28, new(0.5f, 1f), new(330, -117), new(110, 40), bold: true, color: new Color(1f, 0.9f, 0.6f));

        // 일시정지 버튼 확대 (유저 튜닝)
        var pauseBtn = ButtonBox(hud.transform, "PauseButton", "II", 38, new Color(0.15f, 0.16f, 0.2f, 0.9f), new(1, 1), new(-52, -48), new(84, 84));

        Assign(hud, ("progressSlider", pSlider), ("progressText", pText), ("levelSlider", lSlider),
                    ("levelText", lText), ("pauseButton", pauseBtn));
    }

    // 핸들 없는 게이지 Slider — HP Slider(BuildHpSlider)와 동일 구조
    private static Slider SliderGauge(Transform parent, string name, Color fillColor, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        var root = Child(parent, name, anchor, pos, size);
        var slider = root.gameObject.AddComponent<Slider>();
        var bg = Image(root, "Background", new Color(0.08f, 0.08f, 0.08f, 0.95f), C, Vector2.zero, Vector2.zero);
        Stretch(bg.rectTransform);

        var fillArea = Child(root, "Fill Area", C, Vector2.zero, Vector2.zero);
        Stretch(fillArea); fillArea.offsetMin = new(2, 2); fillArea.offsetMax = new(-2, -2);
        var fill = Image(fillArea, "Fill", fillColor, C, Vector2.zero, Vector2.zero);   // 폭 여분 0 — 값 0이면 완전히 빈 바
        fill.rectTransform.anchorMin = new(0, 0); fill.rectTransform.anchorMax = new(0, 1);

        slider.fillRect = fill.rectTransform;
        slider.direction = Slider.Direction.LeftToRight;
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;
        slider.minValue = 0; slider.maxValue = 1; slider.value = 0;
        return slider;
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

    // 선택창만 재조립 (다른 패널 눈튜닝 보존용 별도 메뉴)
    [MenuItem("Tools/Build Selection Panel (원작 레이아웃)")]
    public static void BuildSelectionOnly()
    {
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Font/Kostar SDF 2.asset");
        var panel = Object.FindFirstObjectByType<SkillSelectionPanel>(FindObjectsInactive.Include);
        if (panel == null || font == null) { Debug.LogError("SkillSelectionPanel/폰트 확인"); return; }
        BuildCards(panel);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[InGameUIBuilder] 선택창 재조립 완료 (세로 카드+레벨 바+보유 슬롯) — 씬 저장하세요");
    }

    // ── 3택지 선택창 (원작 #67: 레벨 업 타이틀 + 빨간 레벨 바 + 보유 슬롯 + 세로 카드 3장) ──
    private static void BuildCards(SkillSelectionPanel panel)
    {
        Clear(panel.transform);
        var overlay = Overlay(panel.transform, 0.8f);

        // 상단: 레벨 업 타이틀 + 가득 찬 빨간 바 + 레벨 숫자 배지
        Text(overlay, "Title", "레벨 업", 64, F(0.93f), Vector2.zero, new(500, 80), bold: true, color: new Color(1f, 0.92f, 0.8f));
        var bar = SliderGauge(overlay, "LevelBar", new Color(0.9f, 0.25f, 0.15f), F(0.875f), new(-30, 0), new(560, 30));
        bar.value = 1f;   // 레벨업 순간이므로 항상 가득 (원작)
        var badgeBg = Image(overlay, "LevelBadgeBg", new Color(0.75f, 0.15f, 0.1f), F(0.875f), new(285, 0), new(56, 56));
        badgeBg.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
        var badge = Text(overlay, "LevelBadge", "2", 34, F(0.875f), new(285, 0), new(70, 50), bold: true);

        // 보유 스킬 슬롯 — Active 4 + Passive 2 (원작 배치)
        Text(overlay, "ActiveLabel", "액티브", 26, F(0.80f), new(-250, 0), new(200, 34), color: new Color(1f, 0.7f, 0.6f));
        Text(overlay, "PassiveLabel", "패시브", 26, F(0.80f), new(250, 0), new(200, 34), color: new Color(0.6f, 0.95f, 0.9f));
        var actives = new Image[4]; var passives = new Image[2];
        for (int i = 0; i < 4; i++) actives[i] = SlotIcon(overlay, $"ActiveSlot{i}", new Color(0.45f, 0.2f, 0.2f, 0.9f), new(-355 + i * 95, 0));
        for (int i = 0; i < 2; i++) passives[i] = SlotIcon(overlay, $"PassiveSlot{i}", new Color(0.15f, 0.4f, 0.38f, 0.9f), new(205 + i * 95, 0));

        // 세로 카드 3장 (원작 비율)
        var buttons = new Button[3]; var icons = new Image[3];
        var names = new TMP_Text[3]; var levels = new TMP_Text[3]; var descs = new TMP_Text[3];
        for (int i = 0; i < 3; i++)
        {
            var card = Image(overlay, $"Card{i}", new Color(0.32f, 0.18f, 0.18f, 0.97f), C, new((i - 1) * 330, -140), new(310, 720));
            card.raycastTarget = true;
            buttons[i] = card.gameObject.AddComponent<Button>();
            buttons[i].targetGraphic = card;

            names[i] = Text(card.transform, "Name", "", 40, C, new(0, 280), new(290, 52), bold: true);
            icons[i] = Image(card.transform, "Icon", Color.white, C, new(0, 130), new(170, 170), sprite: false);
            icons[i].preserveAspect = true;
            levels[i] = Text(card.transform, "Level", "", 28, C, new(0, 5), new(290, 38), color: new Color(1f, 0.85f, 0.4f));
            descs[i] = Text(card.transform, "Description", "", 28, C, new(0, -140), new(260, 240), color: new Color(0.95f, 0.9f, 0.85f));
        }

        Assign(panel, ("overlay", overlay.gameObject), ("levelBadge", badge));
        AssignArray(panel, "activeSlots", actives);
        AssignArray(panel, "passiveSlots", passives);
        AssignArray(panel, "buttons", buttons);
        AssignArray(panel, "icons", icons);
        AssignArray(panel, "names", names);
        AssignArray(panel, "levels", levels);
        AssignArray(panel, "descriptions", descs);
        overlay.gameObject.SetActive(false);
    }

    // 보유 슬롯 한 칸 — 프레임(색 유지) + 아이콘(스킬 채움/빈 칸은 비활성)
    private static Image SlotIcon(Transform parent, string name, Color frame, Vector2 pos)
    {
        Image frameImg = Image(parent, name, frame, F(0.74f), pos, new(84, 84));
        var icon = Image(frameImg.transform, "Icon", Color.white, C, Vector2.zero, new(70, 70), sprite: false);
        icon.preserveAspect = true;
        icon.enabled = false;
        return icon;
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
