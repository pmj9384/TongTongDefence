// 아이스볼: 확률 냉동 — a=확률, b=지속, c=이속 감소율 = 받는 피해 증가율
public class IceBallEffect : IOnHitEffect
{
    private readonly System.Random rng;

    public IceBallEffect(System.Random rng) => this.rng = rng;

    public void Apply(Monster target, SkillLevel data)
    {
        if (rng.NextDouble() < data.a)
            target.GetComponent<MonsterStatusEffects>()?.ApplyFreeze(data.b, data.c, data.c);
    }
}
