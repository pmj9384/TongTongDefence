using UnityEngine;

public class PlayerAccountData : ISaveLoad
{
    public DataSourceType SaveDataSouceType => DataSourceType.Local;

    public int BestScore { get; private set; }

    public bool TryUpdateBestScore(int score)
    {
        if (score <= BestScore) return false;
        BestScore = score;
        return true;
    }

    private float bgmVolume;
    public float BgmVolume
    {
        get => bgmVolume;
        set { bgmVolume = Mathf.Clamp(value, 0.0001f, 1f); }
    }

    private float sfxVolume;
    public float SfxVolume
    {
        get => sfxVolume;
        set { sfxVolume = Mathf.Clamp(value, 0.0001f, 1f); }
    }

    // TODO: 게임 특화 데이터 추가

    public PlayerAccountData()
    {
        SaveLoadSystem.Instance.RegisterOnSaveAction(this);
    }

    public void Save()
    {
        var saveData = SaveLoadSystem.Instance.CurrentSaveData.playerAccountDataSave = new();
        saveData.bgmVolume = SoundManager.Instance.bgmVolume;
        saveData.sfxVolume = SoundManager.Instance.sfxVolume;
        saveData.bestScore = BestScore;
    }

    public void Load()
    {
        BgmVolume = 1f;
        SfxVolume = 1f;
        BestScore = 0;
    }

    public void Load(PlayerAccountDataSave saveData)
    {
        if (saveData == null) { Load(); return; }
        BgmVolume = saveData.bgmVolume;
        SfxVolume = saveData.sfxVolume;
        BestScore = saveData.bestScore;
    }
}
