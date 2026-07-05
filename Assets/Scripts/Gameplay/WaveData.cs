using System;

[Serializable]
public struct WaveData
{
    public int killCondition;   // 이 수만큼 처치하면 OnKillConditionMet (스킬 선택 트리거)
    public int monsterCount;
    public int monsterHp;
}
