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

    // 채움 전용 특수 카드 — CSV/테이블에 없음. 드래프트 후보가 3장 미만일 때 빈 자리를 채우고,
    // 획득 시 노멀볼 연사 수 +1 (만렙 없음 → 드래프트가 절대 비지 않는다 = 선택 잠금 버그의 구조적 해결)
    NormalBall,
}

public enum SkillKind
{
    ActiveBall,   // 볼 스킬 — 최대 4개 보유
    Passive,      // 패시브 — 최대 2개 보유
}
