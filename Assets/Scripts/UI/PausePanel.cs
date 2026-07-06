using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 일시정지 패널 (UIElement) — GameStop 진입/이탈 시 GameUIManager가 Show/Hide (템플릿 구독 기존재).
// 원작 구성 재현: 타이틀 / Stage(+전투정보 버튼) / Active 4·Passive 2 슬롯 / "현재 스테이지 드랍" 프레임(모양만) / 이어하기.
// 세로 위치는 전부 "앵커 비율" — 기준 해상도(1080×1920, 폭 기준 스케일)에서 화면비가 달라져도 비율 유지.
// 드랍 보상 내용물은 아웃게임(메타) 영역이라 범위 외 — 프레임만 (유저 결정 2026-07-06).
public class PausePanel : UIElement
{
    private const int ActiveSlots = 4;   // PlayerSkills.MaxActive와 일치 (원작 스크린샷 검증)
    private const int PassiveSlots = 2;

    private GameObject overlay;
    private readonly List<Image> activeIcons = new();
    private readonly List<Image> passiveIcons = new();

    public override void Show()
    {
        if (overlay == null) Build();
        RefreshSlots();
        overlay.SetActive(true);
    }

    public override void Hide()
    {
        if (overlay != null) overlay.SetActive(false);
    }

    private void Resume() => gameManager.SetGameState(GameManager.GameState.GamePlay);

    // 보유 스킬을 슬롯에 채움 — 아이콘 경로는 SkillTable CSV의 icon 컬럼 (SSOT)
    private void RefreshSlots()
    {
        PlayerSkills skills = gameManager.SkillManager.PlayerSkills;
        FillKind(skills, SkillKind.ActiveBall, activeIcons);
        FillKind(skills, SkillKind.Passive, passiveIcons);
    }

    private void FillKind(PlayerSkills skills, SkillKind kind, List<Image> slots)
    {
        int i = 0;
        foreach (SkillId id in skills.Owned(kind))
        {
            if (i >= slots.Count) break;
            slots[i].sprite = Resources.Load<Sprite>(skills.Table[id].iconName);
            slots[i].color = Color.white;
            i++;
        }
        for (; i < slots.Count; i++)   // 빈 슬롯은 어둡게
        {
            slots[i].sprite = RuntimeSprites.White;
            slots[i].color = new Color(0.05f, 0.05f, 0.06f);
        }
    }

    private void Build()
    {
        overlay = CreateChild("Overlay", transform, 0.5f);
        Image dim = overlay.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.9f);   // 원작처럼 게임 화면이 거의 안 비치게
        var overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = overlayRect.offsetMax = Vector2.zero;

        CreateText("Title", 0.87f, 0f, 76, new Color(1f, 0.9f, 0.6f)).text = "일시정지";
        CreateText("Stage", 0.78f, -25f, 34, new Color(0.6f, 0.8f, 1f)).text = "Stage 1  (Normal)";

        // 전투정보 진입 버튼 (Stage 옆 원작 통계 버튼 자리) — 창은 Task #12에서 연결
        GameObject statsGo = CreateChild("CombatInfoButton", overlay.transform, 0.78f);
        Image statsBg = statsGo.AddComponent<Image>();
        statsBg.sprite = RuntimeSprites.White;
        statsBg.color = new Color(0.3f, 0.32f, 0.38f);
        var statsRect = statsGo.GetComponent<RectTransform>();
        statsRect.sizeDelta = new Vector2(52f, 52f);
        statsRect.anchoredPosition = new Vector2(190f, -25f);
        statsGo.AddComponent<Button>();   // TODO(#12): 전투 정보 창 연결
        CreateTextIn(statsGo.transform, "Glyph", 26, Color.white).text = "il";

        // 슬롯 밴드 — Active 4칸(빨간 틀) 왼쪽 / Passive 2칸(청록 틀) 오른쪽
        CreateText("ActiveLabel", 0.665f, -250f, 28, new Color(1f, 0.6f, 0.55f)).text = "Active Skill";
        CreateText("PassiveLabel", 0.665f, 275f, 28, new Color(0.55f, 0.9f, 0.85f)).text = "Passive Skill";
        BuildSlotRow(activeIcons, ActiveSlots, new Color(0.55f, 0.2f, 0.18f), -430f, 0.60f);
        BuildSlotRow(passiveIcons, PassiveSlots, new Color(0.16f, 0.45f, 0.42f), 160f, 0.60f);

        // "현재 스테이지 드랍" 프레임 — 원작 화면 구성 재현 (내용물은 메타 보상이라 범위 외, 모양만)
        CreateText("DropLabel", 0.50f, -320f, 28, new Color(0.75f, 0.75f, 0.9f)).text = "✚ 현재 스테이지 드랍";
        GameObject dropFrame = CreateChild("DropFrame", overlay.transform, 0.35f);
        Image dropBg = dropFrame.AddComponent<Image>();
        dropBg.sprite = RuntimeSprites.White;
        dropBg.color = new Color(0.09f, 0.09f, 0.12f, 0.95f);
        var dropRect = dropFrame.GetComponent<RectTransform>();
        dropRect.sizeDelta = new Vector2(920f, 440f);

        // [이어하기] — 하단
        GameObject buttonGo = CreateChild("ResumeButton", overlay.transform, 0.12f);
        Image buttonBg = buttonGo.AddComponent<Image>();
        buttonBg.sprite = RuntimeSprites.White;
        buttonBg.color = new Color(0.95f, 0.65f, 0.2f);
        var buttonRect = buttonGo.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(380f, 100f);
        buttonGo.AddComponent<Button>().onClick.AddListener(Resume);
        CreateTextIn(buttonGo.transform, "Label", 40, Color.white).text = "이어하기";

        overlay.SetActive(false);
    }

    private void BuildSlotRow(List<Image> slots, int count, Color frameColor, float startX, float anchorY)
    {
        const float size = 116f, gap = 12f;
        for (int i = 0; i < count; i++)
        {
            GameObject frame = CreateChild($"Slot{i}", overlay.transform, anchorY);
            Image frameImage = frame.AddComponent<Image>();
            frameImage.sprite = RuntimeSprites.White;
            frameImage.color = frameColor;
            var frameRect = frame.GetComponent<RectTransform>();
            frameRect.sizeDelta = new Vector2(size, size);
            frameRect.anchoredPosition = new Vector2(startX + i * (size + gap), 0f);

            GameObject icon = CreateChild("Icon", frame.transform, 0.5f);
            Image iconImage = icon.AddComponent<Image>();
            icon.GetComponent<RectTransform>().sizeDelta = new Vector2(size - 12f, size - 12f);
            slots.Add(iconImage);
        }
    }

    // 세로 위치를 앵커 비율로 갖는 자식 생성 — 화면비가 달라도 원작 배치 비율 유지
    private static GameObject CreateChild(string name, Transform parent, float anchorY)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, anchorY);
        return go;
    }

    private Text CreateText(string name, float anchorY, float x, int fontSize, Color color)
    {
        GameObject go = CreateChild(name, overlay.transform, anchorY);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(640f, 80f);
        rect.anchoredPosition = new Vector2(x, 0f);
        return SetupText(go, fontSize, color);
    }

    private static Text CreateTextIn(Transform parent, string name, int fontSize, Color color)
    {
        GameObject go = CreateChild(name, parent, 0.5f);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300f, 80f);
        return SetupText(go, fontSize, color);
    }

    private static Text SetupText(GameObject go, int fontSize, Color color)
    {
        var text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }
}
