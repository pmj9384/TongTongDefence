using System;
using System.Collections.Generic;
using UnityEngine;

// 스킬 시스템의 조정자 — 순수 코어(PlayerLevel/PlayerSkills/SkillDraft)를 소유하고
// 게임 이벤트(처치)와 상태 전환(SkillSelection), UI를 잇는다. 계산·규칙은 전부 코어 몫.
public class SkillManager : InGameManager
{
    [SerializeField] private SkillSelectionPanel selectionPanel;

    public PlayerSkills PlayerSkills => playerSkills;
    public PlayerLevel PlayerLevel => playerLevel;

    private PlayerSkills playerSkills;
    private PlayerLevel playerLevel;
    private readonly System.Random rng = new();
    private int pendingDrafts;   // 연속 레벨업 시 3택지를 연달아 띄우기 위한 큐

    public override void Initialize()
    {
        base.Initialize();

        var csv = Resources.Load<TextAsset>("Tables/SkillTable");
        playerSkills = new PlayerSkills(SkillTableParser.Parse(csv.text));
        playerLevel = new PlayerLevel();

        GameManager.MonsterManager.OnMonsterKilled += HandleMonsterKilled;
    }

    public override void Clear()
    {
        base.Clear();
        GameManager.MonsterManager.OnMonsterKilled -= HandleMonsterKilled;
    }

    private void HandleMonsterKilled()
    {
        pendingDrafts += playerLevel.AddKill();
        // 이미 선택 중이면 큐에만 쌓고, 선택이 끝날 때 이어서 연다
        if (pendingDrafts > 0 && GameManager.CurrentState == GameManager.GameState.GamePlay)
            OpenSelection();
    }

    private void OpenSelection()
    {
        List<SkillId> cards = SkillDraft.Draw(playerSkills, rng);
        if (cards.Count == 0)             // 후보 소진 [가정6] — 선택 스킵
        {
            pendingDrafts = 0;
            return;
        }

        GameManager.SetGameState(GameManager.GameState.SkillSelection);
        selectionPanel.Show(cards, playerSkills, HandleCardPicked);
    }

    private void HandleCardPicked(SkillId picked)
    {
        playerSkills.Acquire(picked);
        selectionPanel.Hide();
        pendingDrafts--;

        if (pendingDrafts > 0)
        {
            OpenSelection();              // 연속 레벨업 — 다음 드래프트 (상태는 SkillSelection 유지)
        }
        else
        {
            GameManager.SetGameState(GameManager.GameState.GamePlay);
        }
    }
}
