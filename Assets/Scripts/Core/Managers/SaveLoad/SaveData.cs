using System;

public abstract class SaveData
{
    public int Version { get; protected set; }
    public abstract SaveData VersionUp();
}

public class SaveDataV1 : SaveData
{
    public PlayerAccountDataSave playerAccountDataSave;
    public StaminaSystemSave staminaSystemSave;
    public SkinUserDataSave skinUserDataSave;

    // [AnimalBreakOut] 게임 전용 시스템
    //public GoldAnimalTokenKeySystemSave goldAnimalTokenKeySystemSave;
    //public PlayerLevelSystemSave playerLevelSystemSave;
    //public StaminaSystemSave staminaSystemSave;
    //public AnimalUserDataListSave animalUserDataTableSave;

    public DateTime saveTime = DateTime.Now;

    public SaveDataV1()
    {
        Version = 1;
    }

    public override SaveData VersionUp()
    {
        return new SaveDataV1();
    }
}
