using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 3택지 카드 UI — 아이콘/이름/레벨/설명 구성 (원작·AnimalBreakOut 스타일 가로 3장).
// 아이콘·설명은 SkillTable CSV(icon/description 컬럼)가 SSOT — 코드에 스킬별 하드코딩 없음.
// timeScale 0 상태에서 동작해야 하므로 애니메이션 없이 즉시 표시/숨김.
// UIElement 미편입 의도: Show(cards, owned, cb)가 데이터 주도형이라 무인자 Show() 계약과 불일치 (설계 검사 판단)
public class SkillSelectionPanel : MonoBehaviour
{
    private const int CardCount = 3;

    private GameObject overlay;
    private readonly Button[] buttons = new Button[CardCount];
    private readonly Image[] icons = new Image[CardCount];
    private readonly Text[] names = new Text[CardCount];
    private readonly Text[] levels = new Text[CardCount];
    private readonly Text[] descriptions = new Text[CardCount];
    private Action<SkillId> onPicked;
    private List<SkillId> currentCards;

    public void Show(List<SkillId> cards, PlayerSkills owned, Action<SkillId> onPicked)
    {
        if (overlay == null) Build();

        this.onPicked = onPicked;
        currentCards = cards;

        for (int i = 0; i < CardCount; i++)
        {
            bool active = i < cards.Count;
            buttons[i].gameObject.SetActive(active);
            if (!active) continue;

            if (cards[i] == SkillId.NormalBall)   // 채움 카드 — 테이블에 없음, 고정 표기
            {
                icons[i].sprite = Resources.Load<Sprite>("Sprites/Balls/Ball_Nomal_Ball");
                names[i].text = "노멀 볼";
                levels[i].text = "볼 +1";
                descriptions[i].text = "기본 볼이 1개 늘어나 연달아 발사됩니다.";
                continue;
            }

            SkillDef def = owned.Table[cards[i]];
            int showLevel = owned.GetLevel(cards[i]) + 1;   // 미보유=Lv1, 보유=현재+1
            string kindTag = def.kind == SkillKind.ActiveBall ? "액티브" : "패시브";

            icons[i].sprite = Resources.Load<Sprite>(def.iconName);
            names[i].text = def.displayName;
            levels[i].text = $"Lv.{showLevel} · {kindTag}";
            descriptions[i].text = def.description;
        }

        overlay.SetActive(true);
    }

    public void Hide()
    {
        if (overlay != null) overlay.SetActive(false);
    }

    // 어두운 오버레이 + 가로 카드 3장 생성 (1회)
    private void Build()
    {
        overlay = CreateChild("Overlay", transform);
        Image dim = overlay.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.6f);
        Stretch(overlay.GetComponent<RectTransform>());

        for (int i = 0; i < CardCount; i++)
        {
            int index = i;   // 클로저 캡처용
            GameObject card = CreateChild($"Card{i}", overlay.transform);
            var rect = card.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(320f, 460f);                   // 세로로 긴 카드형
            rect.anchoredPosition = new Vector2((i - 1) * 350f, 0f);    // 좌/중/우 가로 배치

            Image bg = card.AddComponent<Image>();
            bg.color = new Color(0.16f, 0.22f, 0.2f, 0.95f);
            buttons[i] = card.AddComponent<Button>();
            buttons[i].onClick.AddListener(() => onPicked?.Invoke(currentCards[index]));

            // 아이콘 (카드 상단)
            GameObject iconGo = CreateChild("Icon", card.transform);
            var iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(150f, 150f);
            iconRect.anchoredPosition = new Vector2(0f, 110f);
            icons[i] = iconGo.AddComponent<Image>();
            icons[i].preserveAspect = true;
            icons[i].raycastTarget = false;

            names[i] = CreateText(card.transform, "Name", 38, FontStyle.Bold,
                new Vector2(0f, -10f), new Vector2(300f, 50f), Color.white);
            levels[i] = CreateText(card.transform, "Level", 26, FontStyle.Normal,
                new Vector2(0f, -55f), new Vector2(300f, 36f), new Color(1f, 0.85f, 0.4f));
            descriptions[i] = CreateText(card.transform, "Description", 24, FontStyle.Normal,
                new Vector2(0f, -140f), new Vector2(280f, 120f), new Color(0.85f, 0.88f, 0.85f));
        }

        overlay.SetActive(false);
    }

    private static Text CreateText(Transform parent, string name, int fontSize, FontStyle style,
                                   Vector2 position, Vector2 size, Color color)
    {
        GameObject go = CreateChild(name, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        var text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.raycastTarget = false;   // 카드 버튼 클릭 방해 금지
        return text;
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
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
}
