// 스킬 식별자/종류 — 기획서 PDF의 10종 고정.
// 순수 코어(TongTong.SkillCore): 엔진 의존 없음, EditMode 테스트 대상.
public enum SkillId
{
    FireBall,
    IceBall,
    LaserBall,
    GhostBall,
    ClusterBall,
    TinHeart,
    MagicMirror,
    AmethystDagger,
    EmeraldDagger,
    LastMatch,
}

public enum SkillKind
{
    ActiveBall,   // 볼 스킬 — 최대 4개 보유
    Passive,      // 패시브 — 최대 2개 보유
}
