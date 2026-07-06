// 스킬 정의 — 수치의 SSOT는 Resources/Tables/SkillTable.csv (기획자가 수치만 수정 가능).
// a/b/c의 스킬별 의미 (CSV와 이 표가 계약):
//   FireBall:   a=화상 지속(초)      b=최대 중첩          c=중첩당 초당 피해
//   IceBall:    a=냉동 확률(0~1)     b=냉동 지속(초)      c=이속 감소율 = 받는 피해 증가율 (PDF상 동일 수치)
//   LaserBall:  a=같은 행 피해
//   GhostBall:  (파라미터 없음 — 관통은 거동)
//   ClusterBall:a=파편 생성 확률     b=파편 피해
//   TinHeart:   a=노멀볼 추가 피해율
//   MagicMirror:a=벽 반사당 다음 타격 증가율
//   AmethystDagger: a=전면 타격 치명타 확률 증가 (적당 1회)
//   EmeraldDagger:  a=후면 타격 치명타 확률 증가 (적당 1회)
//   LastMatch:  a=사망 폭발 피해
public struct SkillLevel
{
    public int ballDamage;   // 액티브 볼 기본 데미지 (패시브는 0)
    public float a;
    public float b;
    public float c;
}

public class SkillDef
{
    public SkillId id;
    public SkillKind kind;
    public string displayName;
    public string description;   // 카드 UI 설명 문구 (기획자 수정 영역)
    public string iconName;      // Resources 상대 경로 — 스킬↔아이콘 매핑의 SSOT (코드 하드코딩 대체)
    public SkillLevel[] levels;   // 길이 3 (Lv1~3), 인덱스 = 레벨-1

    public SkillLevel GetLevel(int level) => levels[level - 1];
}
