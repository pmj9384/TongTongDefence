using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

// 3택지 카드 UI — 카드 내용 채우기/선택 전달만. 배치는 씬 소관 (에디터-네이티브).
// 아이콘·설명은 SkillTable CSV(icon/description 컬럼)가 SSOT.
// UIElement 미편입 의도: Show(cards, owned, cb)가 데이터 주도형이라 무인자 Show() 계약과 불일치.
public class SkillSelectionPanel : MonoBehaviour
{
    [Header("씬 참조 (빌더 조립)")]
    [SerializeField] private GameObject overlay;
    [SerializeField] private TMP_Text levelBadge;        // 레벨 바 오른쪽 숫자 (원작 #67)
    [SerializeField] private Image[] activeSlots;        // 보유 액티브 4칸
    [SerializeField] private Image[] passiveSlots;       // 보유 패시브 2칸
    [SerializeField] private Button[] buttons;
    [SerializeField] private Image[] icons;
    [SerializeField] private TMP_Text[] names;
    [SerializeField] private TMP_Text[] levels;
    [SerializeField] private TMP_Text[] descriptions;

    private Action<SkillId> onPicked;
    private List<SkillId> currentCards;

    private void Awake()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;   // 클로저 캡처용
            buttons[i].onClick.AddListener(() => onPicked?.Invoke(currentCards[index]));
        }
    }

    public void Show(List<SkillId> cards, PlayerSkills owned, int playerLevel, Action<SkillId> onPicked)
    {
        this.onPicked = onPicked;
        currentCards = cards;

        levelBadge.text = playerLevel.ToString();
        FillSlots(activeSlots, owned, SkillKind.ActiveBall);
        FillSlots(passiveSlots, owned, SkillKind.Passive);

        for (int i = 0; i < buttons.Length; i++)
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

    // 보유 스킬 슬롯 채우기 (퍼즈 패널과 동일 문법) — 빈 칸은 아이콘 숨김
    private void FillSlots(Image[] slots, PlayerSkills owned, SkillKind kind)
    {
        int i = 0;
        foreach (SkillId id in owned.Owned(kind))
        {
            if (i >= slots.Length) break;
            slots[i].enabled = true;
            slots[i].sprite = Resources.Load<Sprite>(owned.Table[id].iconName);
            i++;
        }
        for (; i < slots.Length; i++)
            slots[i].enabled = false;
    }

    public void Hide() => overlay.SetActive(false);
}
