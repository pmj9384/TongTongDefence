using AYellowpaper.SerializedCollections;
using UnityEditor;
using UnityEngine;

// 사운드 1회 배선 도구 — SerializedDictionary는 손 YAML 금지 규약 대상(복잡 직렬화)이라 에디터 API로.
// 오디오소스 생성 + 클립 딕셔너리 채우기 + 참조 연결을 원버튼으로 (재실행 안전)
public static class SoundWiringTool
{
    [MenuItem("Tools/Wire Sounds (BGM+SFX 배선)")]
    public static void Wire()
    {
        var sm = Object.FindFirstObjectByType<SoundManager>(FindObjectsInactive.Include);
        if (sm == null) { Debug.LogError("씬에 SoundManager 없음"); return; }

        var bgm = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/Sounds/ingame_bgm.wav");
        var bounce = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/Sounds/ball_bounce.wav");
        var hit = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/Sounds/ball_hit.wav");
        if (bgm == null || bounce == null || hit == null) { Debug.LogError("Sounds 폴더 클립 확인 (임포트 대기?)"); return; }

        // 오디오소스 컨테이너 (BGM 1 + SFX 채널 4)
        Transform player = sm.transform.Find("AudioSourcePlayer");
        if (player == null)
        {
            player = new GameObject("AudioSourcePlayer").transform;
            player.SetParent(sm.transform, false);
        }
        foreach (Transform c in player) Object.DestroyImmediate(c.gameObject);   // 재실행 시 재조립

        var so = new SerializedObject(sm);
        so.FindProperty("audioSourcePlayer").objectReferenceValue = player.gameObject;

        var bgmSrc = new GameObject("BGM").AddComponent<AudioSource>();
        bgmSrc.transform.SetParent(player, false);
        so.FindProperty("bgmAudioSource").objectReferenceValue = bgmSrc;

        var list = so.FindProperty("sfxAudioSourceList");
        list.arraySize = 4;
        for (int i = 0; i < 4; i++)
        {
            var src = new GameObject($"SFX{i}").AddComponent<AudioSource>();
            src.transform.SetParent(player, false);
            list.GetArrayElementAtIndex(i).objectReferenceValue = src;
        }

        FillDict(so, "bgmClips", new (int, Object)[] { ((int)BgmClipId.IngameBGM, bgm) });
        FillDict(so, "sfxClips", new (int, Object)[] {
            ((int)SfxClipId.BallBounce, bounce), ((int)SfxClipId.BallHit, hit) });

        so.ApplyModifiedProperties();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(sm.gameObject.scene);
        Debug.Log("[SoundWiringTool] 배선 완료 — BGM 1 + SFX 2, 채널 4. 씬 저장하세요");
    }

    private static void FillDict(SerializedObject so, string field, (int key, Object clip)[] entries)
    {
        var listProp = so.FindProperty($"{field}._serializedList");
        listProp.arraySize = entries.Length;
        for (int i = 0; i < entries.Length; i++)
        {
            var kvp = listProp.GetArrayElementAtIndex(i);
            kvp.FindPropertyRelative("Key").enumValueIndex = entries[i].key;
            kvp.FindPropertyRelative("Value").objectReferenceValue = entries[i].clip;
        }
    }
}
