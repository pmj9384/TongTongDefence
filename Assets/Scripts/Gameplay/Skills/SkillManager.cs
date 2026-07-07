using System;
using System.Collections.Generic;
using UnityEngine;

// 스킬 시스템의 조정자 — 순수 코어(PlayerLevel/PlayerSkills/SkillDraft/DamageCalculator)를 소유하고
// 게임 이벤트(처치·볼 히트)와 상태 전환(SkillSelection), UI를 잇는다. 계산·규칙은 전부 코어 몫.
public class SkillManager : InGameManager
{
    [SerializeField] private SkillSelectionPanel selectionPanel;
    [SerializeField] private GameObject fragmentPrefab;     // 클러스터 파편볼

    public PlayerSkills PlayerSkills => playerSkills;
    public PlayerLevel PlayerLevel => playerLevel;

    private PlayerSkills playerSkills;
    private PlayerLevel playerLevel;
    private readonly System.Random rng = new();
    private int pendingDrafts;                              // 연속 레벨업 시 3택지를 연달아 띄우기 위한 큐

    [SerializeField] private int initialNormalBalls = 5;   // 시작 노멀볼 수 (원작 재확인: 5발 — 2026-07-07 정정)

    // 발사 볼 인벤토리 — 보유 볼 각각이 개체 (필드에 나가 있거나 대기). 규칙은 BallInventory(순수 코어) 몫
    private BallInventory ballInventory;
    public int NormalBallLevel { get; private set; } = 1;  // 노멀볼 레벨 (전투 정보 ◆xN) — 채움 카드로 개수와 함께 성장
    private readonly HashSet<Monster> critConsumed = new(); // 단검 "적당 1회" 소모 기록

    // 스킬 "행동"은 클래스 단위로 분리(SRP) — 여기는 디스패치만
    private Dictionary<SkillId, IOnHitEffect> onHitEffects;
    private LastMatchEffect lastMatch;
    private UnityEngine.Pool.ObjectPool<GameObject> fragmentPool;

    public override void Initialize()
    {
        base.Initialize();

        var csv = Resources.Load<TextAsset>("Tables/SkillTable");
        playerSkills = new PlayerSkills(SkillTableParser.Parse(csv.text));
        playerLevel = new PlayerLevel();

        ballInventory = new BallInventory();
        for (int i = 0; i < initialNormalBalls; i++)
            ballInventory.Add(null);   // 시작 구성: 노멀볼 N발

        fragmentPool = GameManager.ObjectPool.CreateObjectPool(
            fragmentPrefab,
            createFunc: () => Instantiate(fragmentPrefab),
            onGet: o => o.SetActive(true),
            onRelease: o => o.SetActive(false));

        onHitEffects = new Dictionary<SkillId, IOnHitEffect>
        {
            { SkillId.FireBall, new FireBallEffect() },
            { SkillId.IceBall, new IceBallEffect(rng) },
            { SkillId.LaserBall, new LaserBallEffect(GameManager.MonsterManager) },
            { SkillId.ClusterBall, new ClusterBallEffect(rng, SpawnFragment) },
            // GhostBall은 온히트 효과 없음 — 관통은 Ball의 레이어/센서 거동
        };
        lastMatch = new LastMatchEffect(GameManager.FieldManager);

        GameManager.MonsterManager.OnMonsterKilled += HandleMonsterKilled;
        GameManager.MonsterManager.OnMonsterDespawned += HandleMonsterDespawned;
        GameManager.BallManager.OnBallHitMonster += HandleBallHit;
    }

    public override void Clear()
    {
        base.Clear();
        GameManager.MonsterManager.OnMonsterKilled -= HandleMonsterKilled;
        GameManager.MonsterManager.OnMonsterDespawned -= HandleMonsterDespawned;
        GameManager.BallManager.OnBallHitMonster -= HandleBallHit;
    }

    // 도달 돌진으로 소멸한 몬스터 — 처치가 아니라서 HandleMonsterKilled를 안 타므로,
    // 단검 "적당 1회" 기록을 여기서 지워야 풀 재사용 몬스터가 소모 상태로 시작하지 않는다 (검수 v2 #2)
    private void HandleMonsterDespawned(Monster monster) => critConsumed.Remove(monster);

    // ── 발사 로테이션 ─────────────────────────────────────────────

    // 인벤토리 대기열 앞의 볼을 발사용으로 꺼냄 — 대기 없으면 false (전부 필드에 나가 있음).
    // 레벨/데미지는 "발사 시점"에 조회 — 비행 중 레벨업이 다음 발사부터 반영된다
    public bool TryGetNextLoadout(out BallLoadout loadout)
    {
        if (!ballInventory.TryTakeNext(out SkillId? skill))
        {
            loadout = default;
            return false;
        }

        if (skill == null)
        {
            loadout = BallLoadout.Normal;
            return true;
        }

        int level = playerSkills.GetLevel(skill.Value);
        loadout = new BallLoadout
        {
            skill = skill,
            level = level,
            damage = playerSkills.Table[skill.Value].GetLevel(level).ballDamage,
        };
        return true;
    }

    // 회수된 볼을 대기열 뒤로 — 먼저 회수된 순 재발사 (원작 규칙)
    public void ReturnBall(SkillId? skill) => ballInventory.Return(skill);

    // ── 데미지 파이프라인 ─────────────────────────────────────────

    private void HandleBallHit(Ball ball, Collider2D monsterCollider, Vector2 hitNormal)
    {
        Monster monster = monsterCollider.GetComponent<Monster>();
        if (monster == null) return;

        MonsterStatusEffects status = monster.GetComponent<MonsterStatusEffects>();

        var ctx = new DamageCalculator.Context
        {
            baseDamage = ball.BaseDamage,
            isNormalBall = ball.ActiveSkill == null,
            tinHeartBonus = PassiveValue(SkillId.TinHeart),
            mirrorPerBounce = PassiveValue(SkillId.MagicMirror),
            wallBounces = ball.WallBounceCount,
            targetFrozen = status != null && status.IsFrozen,
            frozenBonus = status != null ? status.FrozenDamageBonus : 0f,
            critChance = CritChanceFor(monster, hitNormal),
            critMultiplier = 1.5f,  // 기획서: 치명타 데미지율 50%
        };

        monster.TakeDamage(DamageCalculator.Calc(ctx, rng, out bool isCrit), isCrit, ball.ActiveSkill);
        ApplyOnHitEffect(ball, monster, status);
    }

    // 볼 타입별 온히트 효과 — 행동은 Effects/ 클래스, 수치는 SkillTable(CSV) 현재 레벨 값
    private void ApplyOnHitEffect(Ball ball, Monster monster, MonsterStatusEffects status)
    {
        if (ball.ActiveSkill == null) return;
        if (!onHitEffects.TryGetValue(ball.ActiveSkill.Value, out IOnHitEffect effect)) return;

        effect.Apply(monster, playerSkills.Table[ball.ActiveSkill.Value].GetLevel(ball.SkillLevel));
    }

    // 클러스터 파편 스폰/회수 — 풀 소유는 매니저, 효과 클래스는 델리게이트만 사용
    private void SpawnFragment(Vector2 position, int damage)
    {
        FragmentBall fragment = fragmentPool.Get().GetComponent<FragmentBall>();
        fragment.OnDespawn += HandleFragmentDespawn;
        fragment.Launch(position, damage, rng);
    }

    private void HandleFragmentDespawn(FragmentBall fragment)
    {
        fragment.OnDespawn -= HandleFragmentDespawn;
        fragmentPool.Release(fragment.gameObject);
    }

    // 보유 패시브의 현재 레벨 a값 (미보유 = 0)
    private float PassiveValue(SkillId id)
    {
        int level = playerSkills.GetLevel(id);
        return level == 0 ? 0f : playerSkills.Table[id].GetLevel(level).a;
    }

    // 단검 치명타 [가정3]: 충돌 노멀은 표면→볼 방향이므로,
    // 노멀이 아래(-y) = 볼이 아래에서 타격 = 전면(자수정) / 위(+y) = 후면(에메랄드). 적당 1회만.
    private float CritChanceFor(Monster monster, Vector2 hitNormal)
    {
        if (critConsumed.Contains(monster)) return 0f;

        float bonus = 0f;
        if (hitNormal.y < -0.3f) bonus = PassiveValue(SkillId.AmethystDagger);
        else if (hitNormal.y > 0.3f) bonus = PassiveValue(SkillId.EmeraldDagger);

        if (bonus > 0f) critConsumed.Add(monster);
        return bonus;
    }

    // ── 레벨업 → 3택지 ────────────────────────────────────────────

    private void HandleMonsterKilled(Monster monster)
    {
        critConsumed.Remove(monster);   // 풀 재사용 대비 — 죽은 몬스터의 1회 소모 기록 해제

        // 마지막 성냥: 사망 폭발 (연쇄 사망 시 재귀적으로 다시 발동 — 의도된 연쇄)
        int matchLevel = playerSkills.GetLevel(SkillId.LastMatch);
        if (matchLevel > 0)
            lastMatch.Explode(monster.transform.position, playerSkills.Table[SkillId.LastMatch].GetLevel(matchLevel));

        pendingDrafts += playerLevel.AddKill();
        // 이미 선택 중이면 큐에만 쌓고, 선택이 끝날 때 이어서 연다.
        // 즉시 열지 않고 잠깐 지연 — 레벨 게이지가 "꽉 차는" 연출을 보여준 뒤 창이 뜬다 (원작 시퀀스)
        if (pendingDrafts > 0 && GameManager.CurrentState == GameManager.GameState.GamePlay && openDelay == null)
            openDelay = StartCoroutine(OpenSelectionAfterGaugeFill());
    }

    private Coroutine openDelay;
    private const float SelectionOpenDelay = 0.35f;  // 게이지 채움 연출 시간 — 0.6은 "한 템포 늦음"으로 체감 (유저 2026-07-07)

    private System.Collections.IEnumerator OpenSelectionAfterGaugeFill()
    {
        yield return new WaitForSeconds(SelectionOpenDelay);   // 스케일 시간 — 이 동안 게임은 계속 돈다 (원작)
        openDelay = null;
        if (pendingDrafts > 0 && GameManager.CurrentState == GameManager.GameState.GamePlay)
            OpenSelection();
    }

    private void OpenSelection()
    {
        List<SkillId> cards = SkillDraft.Draw(playerSkills, rng);
        if (cards.Count == 0)           // 채움 카드 덕에 사실상 불가능 — 방어적 안전망
        {
            pendingDrafts = 0;
            // 연속 레벨업 체인 "도중" 소진이면 상태가 SkillSelection에 잠긴 채 방치되던 실버그 —
            // 복귀시켜 줄 사람이 없으므로 여기서 직접 GamePlay로
            if (GameManager.CurrentState == GameManager.GameState.SkillSelection)
                GameManager.SetGameState(GameManager.GameState.GamePlay);
            return;
        }

        GameManager.SetGameState(GameManager.GameState.SkillSelection);
        selectionPanel.Show(cards, playerSkills, playerLevel.Level, HandleCardPicked);
    }

    private void HandleCardPicked(SkillId picked)
    {
        if (picked == SkillId.NormalBall)
        {
            NormalBallLevel++;              // 채움 카드 — 노멀볼 레벨과 개수가 함께 성장 (◆xN)
            ballInventory.Add(null);
        }
        else
        {
            bool isNew = !playerSkills.Has(picked);
            playerSkills.Acquire(picked);
            if (isNew && playerSkills.Table[picked].kind == SkillKind.ActiveBall)
                ballInventory.Add(picked);  // 새 액티브 볼 = 인벤토리에 볼 1개 추가
        }

        selectionPanel.Hide();
        pendingDrafts--;

        if (pendingDrafts > 0)
        {
            OpenSelection();            // 연속 레벨업 — 다음 드래프트 (상태는 SkillSelection 유지)
        }
        else
        {
            GameManager.SetGameState(GameManager.GameState.GamePlay);
        }
    }
}
