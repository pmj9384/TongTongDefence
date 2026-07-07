// 볼 타격 시 발동하는 스킬 "행동"의 계약 — 스킬 하나 = 클래스 하나 (SRP).
// 수치(data)는 CSV에서 오고, 행동만 여기 구현. SkillManager는 디스패치 테이블로 호출만 한다.
public interface IOnHitEffect
{
    void Apply(Monster target, SkillLevel data);
}
