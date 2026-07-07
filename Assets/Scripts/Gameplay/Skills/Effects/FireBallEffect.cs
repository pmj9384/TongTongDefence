// 파이어볼: 타격 시 화상 부여 — a=지속, b=최대 중첩, c=중첩당 초당 피해
public class FireBallEffect : IOnHitEffect
{
    public void Apply(Monster target, SkillLevel data)
    {
        target.GetComponent<MonsterStatusEffects>()?.ApplyBurn(data.a, (int)data.b, data.c);
    }
}
