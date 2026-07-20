using UnityEditor;
using UnityEngine;

// TongTongDefence 고유 치트 — CheatWindow(base)의 partial 훅 DrawProjectCheats()를 채운다.
// 스킬은 이 프로젝트 전용이라 base(템플릿)에 두지 않고 여기서만 잇는다.
public partial class CheatWindow
{
    private SkillId skillToLevel = SkillId.FireBall;

    partial void DrawProjectCheats()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("스킬", EditorStyles.boldLabel);
        skillToLevel = (SkillId)EditorGUILayout.EnumPopup("대상", skillToLevel);
        if (GUILayout.Button("선택 스킬 +1레벨 (미보유면 획득 / 만렙 액티브볼은 +1개)"))
        {
            // 디버그라 Find 허용 — SkillManager는 주입식이라 static Instance가 없다
            SkillManager skills = FindAnyObjectByType<SkillManager>();
            if (skills != null) skills.DebugLevelUp(skillToLevel);
            else Debug.LogWarning("[Cheat] SkillManager 없음 — 인게임 씬에서만 동작");
        }
    }
}
