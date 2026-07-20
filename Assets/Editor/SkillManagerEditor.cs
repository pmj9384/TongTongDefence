using System;
using UnityEditor;
using UnityEngine;

// SkillManager 인스펙터에 개발용 디버그 버튼을 얹는다 — Play 중 스킬을 원하는 만큼 +1(미보유면 획득)해서
// 만렙 도달·"+1개" 카드 검증을 빠르게. 에디터 전용이라 빌드와 무관.
[CustomEditor(typeof(SkillManager))]
public class SkillManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("── DEBUG: 스킬 레벨업 ──", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Play 중에만 동작합니다. 버튼 1회 = 해당 스킬 +1레벨(미보유면 획득).", MessageType.Info);
            return;
        }

        var manager = (SkillManager)target;
        foreach (SkillId id in Enum.GetValues(typeof(SkillId)))
        {
            if (GUILayout.Button($"+1   {id}"))
                manager.DebugLevelUp(id);
        }
    }
}
