using UnityCommunity.UnitySingleton;
using UnityEngine;

public class GameDataManager : PersistentMonoSingleton<GameDataManager>
{
    public PlayerAccountData PlayerAccountData { get; private set; }
    public StaminaSystem StaminaSystem { get; private set; }
    public SkinUserData SkinUserData { get; private set; }

    public override void InitializeSingleton()
    {
        base.InitializeSingleton();
        SaveLoadSystem.Instance.Load();

        PlayerAccountData = new();
        PlayerAccountData.Load(SaveLoadSystem.Instance.CurrentSaveData.playerAccountDataSave);

        StaminaSystem = new();
        StaminaSystem.Load(SaveLoadSystem.Instance.CurrentSaveData.staminaSystemSave);

        SkinUserData = new();
        SkinUserData.Load(SaveLoadSystem.Instance.CurrentSaveData.skinUserDataSave);
    }

    public Coroutine StartStaminaRecovery()
    {
        return StartCoroutine(StaminaSystem.CoRecovery());
    }
}
