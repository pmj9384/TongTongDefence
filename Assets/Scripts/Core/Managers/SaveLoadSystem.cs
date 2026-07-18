using Newtonsoft.Json;
using System;
using System.IO;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using SaveDataVC = SaveDataV1;

public class SaveLoadSystem : PersistentMonoSingleton<SaveLoadSystem>
{
    public static int SaveDataVersion { get; private set; } = 1;

    public SaveDataVC CurrentSaveData { get; set; }

    public static string CurrentSaveFileName => "SaveFile.json";

    public static string SavePathDirectory => $"{Application.persistentDataPath}/Save";

    public Action onApplicationQuitSave;

    public override void InitializeSingleton()
    {
        base.InitializeSingleton();
        CurrentSaveData = new();
    }

    private static JsonSerializerSettings settings = new JsonSerializerSettings
    {
        Formatting = Formatting.Indented,
        TypeNameHandling = TypeNameHandling.All,
    };

    public void Save()
    {
        OnApplicationQuitSave();

        if (!Directory.Exists(SavePathDirectory))
            Directory.CreateDirectory(SavePathDirectory);

        var path = Path.Combine(SavePathDirectory, CurrentSaveFileName);
        var json = JsonConvert.SerializeObject(CurrentSaveData, settings);
        File.WriteAllText(path, json);
    }

    public void Load()
    {
        var path = Path.Combine(SavePathDirectory, CurrentSaveFileName);

        if (!File.Exists(path))
        {
            Debug.Log($"Save file not found, using defaults.");
            return;
        }

        var json = File.ReadAllText(path);
        var saveData = JsonConvert.DeserializeObject<SaveData>(json, settings);
        while (saveData.Version < SaveDataVersion)
        {
            saveData = saveData.VersionUp();
        }

        CurrentSaveData = saveData as SaveDataVC;
    }

    private void OnApplicationQuitSave()
    {
        onApplicationQuitSave?.Invoke();
    }

    public void RegisterOnSaveAction(ISaveLoad target)
    {
        onApplicationQuitSave += target.Save;
    }

    private void OnApplicationQuit()
    {
        Save();
    }

#if !UNITY_EDITOR
    private void OnApplicationPause(bool pause)
    {
        if (pause)
            Save();
    }
#endif
}
