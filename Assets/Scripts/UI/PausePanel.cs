using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 일시정지 패널 (UIElement) — GameStop 진입/이탈 시 GameUIManager가 Show/Hide (템플릿 구독 기존재).
// 원작 구성: "일시정지" 타이틀 + Stage + Active 4칸/Passive 2칸 보유 스킬 슬롯 + [이어하기].
// 홈/설정/스테이지 드랍 그리드는 아웃게임(메타) 영역이라 범위 외.
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
        overlay = CreateChild("Overlay", transform);
        Image dim = overlay.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.8f);
        var overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = overlayRect.offsetMax = Vector2.zero;

        CreateText(overlay.transform, "Title", 64, new Vector2(0f, 320f), new Color(1f, 0.9f, 0.6f)).text = "일시정지";
        CreateText(overlay.transform, "Stage", 30, new Vector2(0f, 240f), new Color(0.6f, 0.8f, 1f)).text = "Stage 1  (Normal)";

        // 슬롯 밴드 — Active 4칸(빨간 틀) 왼쪽, Passive 2칸(청록 틀) 오른쪽 (원작 배치)
        CreateText(overlay.transform, "ActiveLabel", 24, new Vector2(-170f, 160f), new Color(1f, 0.6f, 0.55f)).text = "Active Skill";
        CreateText(overlay.transform, "PassiveLabel", 24, new Vector2(210f, 160f), new Color(0.55f, 0.9f, 0.85f)).text = "Passive Skill";
        BuildSlotRow(activeIcons, ActiveSlots, new Color(0.55f, 0.2f, 0.18f), -320f);
        BuildSlotRow(passiveIcons, PassiveSlots, new Color(0.16f, 0.45f, 0.42f), 130f);

        // [이어하기]
        GameObject buttonGo = CreateChild("ResumeButton", overlay.transform);
        Image buttonBg = buttonGo.AddComponent<Image>();
        buttonBg.sprite = RuntimeSprites.White;
        buttonBg.color = new Color(0.95f, 0.65f, 0.2f);
        var buttonRect = buttonGo.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(320f, 92f);
        buttonRect.anchoredPosition = new Vector2(0f, -220f);
        buttonGo.AddComponent<Button>().onClick.AddListener(Resume);
        CreateText(buttonGo.transform, "Label", 38, Vector2.zero, Color.white).text = "이어하기";

        overlay.SetActive(false);
    }

    private void BuildSlotRow(List<Image> slots, int count, Color frameColor, float startX)
    {
        const float size = 88f, gap = 8f;
        for (int i = 0; i < count; i++)
        {
            GameObject frame = CreateChild($"Slot{i}", overlay.transform);
            Image frameImage = frame.AddComponent<Image>();
            frameImage.sprite = RuntimeSprites.White;
            frameImage.color = frameColor;
            var frameRect = frame.GetComponent<RectTransform>();
            frameRect.sizeDelta = new Vector2(size, size);
            frameRect.anchoredPosition = new Vector2(startX + i * (size + gap), 90f);

            GameObject icon = CreateChild("Icon", frame.transform);
            Image iconImage = icon.AddComponent<Image>();
            var iconRect = icon.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(size - 10f, size - 10f);
            slots.Add(iconImage);
        }
    }

    private static GameObject CreateChild(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static Text CreateText(Transform parent, string name, int fontSize, Vector2 position, Color color)
    {
        GameObject go = CreateChild(name, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(560f, 80f);
        rect.anchoredPosition = position;
        var text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }
}
