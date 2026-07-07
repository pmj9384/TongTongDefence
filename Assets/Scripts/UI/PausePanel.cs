using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 일시정지 패널 (UIElement) — 슬롯 채우기/이어하기 로직만. 배치는 씬 소관 (에디터-네이티브).
// GameStop 진입/이탈 시 GameUIManager가 Show/Hide (템플릿 구독 기존재).
// 드랍 프레임은 원작 화면 구성 재현용 모양만 — 보상은 아웃게임 영역이라 범위 외 (유저 결정).
public class PausePanel : UIElement
{
    [Header("씬 참조")]
    [SerializeField] private GameObject overlay;
    [SerializeField] private List<Image> activeIcons;    // Active 슬롯 4칸의 Icon 이미지들
    [SerializeField] private List<Image> passiveIcons;   // Passive 슬롯 2칸
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button combatInfoButton;    // 전투 정보 창 열기 (#12)

    private void Awake()
    {
        resumeButton.onClick.AddListener(Resume);
        combatInfoButton.onClick.AddListener(OpenCombatInfo);
    }

    // 전투 정보로 전환 — 상태는 GameStop 유지, 창만 교체 (닫으면 CombatInfoPanel이 퍼즈를 되연다)
    private void OpenCombatInfo()
    {
        Hide();
        gameUIManager.ShowUIElement(UIElementEnums.CombatInfoPanel);
    }

    public override void Show()
    {
        RefreshSlots();
        overlay.SetActive(true);
    }

    public override void Hide() => overlay.SetActive(false);

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
            slots[i].enabled = true;
            slots[i].sprite = Resources.Load<Sprite>(skills.Table[id].iconName);
            i++;
        }
        for (; i < slots.Count; i++)   // 빈 슬롯 = 아이콘 숨김 (어두운 안판이 보임 — 선택창과 동일 문법)
            slots[i].enabled = false;
    }
}
