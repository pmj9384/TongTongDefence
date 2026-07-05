using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 3택지 카드 UI — MVP: 카드 버튼을 코드로 생성 (씬 의존 최소화, 이후 결과 UI 태스크에서 프리팹/UISystem 통합).
// timeScale 0 상태에서 동작해야 하므로 애니메이션 없이 즉시 표시/숨김.
public class SkillSelectionPanel : MonoBehaviour
{
    private const int CardCount = 3;

    private GameObject overlay;
    private readonly Button[] buttons = new Button[CardCount];
    private readonly Text[] labels = new Text[CardCount];
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

            SkillDef def = owned.Table[cards[i]];
            int showLevel = owned.GetLevel(cards[i]) + 1;   // 미보유=Lv1, 보유=현재+1
            string kindTag = def.kind == SkillKind.ActiveBall ? "액티브" : "패시브";
            labels[i].text = $"{def.displayName}\nLv.{showLevel}  ({kindTag})";
        }

        overlay.SetActive(true);
    }

    public void Hide()
    {
        if (overlay != null) overlay.SetActive(false);
    }

    // 어두운 오버레이 + 세로 카드 3장 생성 (1회)
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
            rect.anchoredPosition = new Vector2((i - 1) * 350f, 0f);    // 좌/중/우 가로 배치 (원작·AnimalBreakOut 스타일)

            Image bg = card.AddComponent<Image>();
            bg.color = new Color(0.16f, 0.22f, 0.2f, 0.95f);
            buttons[i] = card.AddComponent<Button>();
            buttons[i].onClick.AddListener(() => onPicked?.Invoke(currentCards[index]));

            GameObject textGo = CreateChild("Label", card.transform);
            Stretch(textGo.GetComponent<RectTransform>());
            Text label = textGo.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 44;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            labels[i] = label;
        }

        overlay.SetActive(false);
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
