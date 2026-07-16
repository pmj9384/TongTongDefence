using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameDataManager))]
public class GameDataManagerEditor : Editor
{
    private int coinAmount = 1000;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("플레이 모드에서만 동작합니다.", MessageType.Info);
            return;
        }

        var manager = (GameDataManager)target;
        var data = manager.PlayerAccountData;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("── Debug: 코인 ──", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"현재 코인: {data.Coins}");

        coinAmount = EditorGUILayout.IntField("조절량", coinAmount);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button($"+{coinAmount} 코인"))
        {
            data.AddCoins(coinAmount);
            SaveLoadSystem.Instance.Save();
        }
        if (GUILayout.Button("코인 초기화"))
        {
            data.SpendCoin(data.Coins);
            SaveLoadSystem.Instance.Save();
        }
        EditorGUILayout.EndHorizontal();
    }
}
