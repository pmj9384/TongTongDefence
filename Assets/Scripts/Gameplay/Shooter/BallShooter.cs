using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BallShooter
{
    private readonly BallManager ballManager;
    private readonly Transform origin;
    private readonly float shootCooldown;
    private readonly float ballSpeed;
    private readonly ObjectPool<GameObject> ballPool;

    private float cooldownTimer;
    private readonly Dictionary<string, Sprite> spriteCache = new();

    public BallShooter(BallManager ballManager, GameObject ballPrefab, Transform origin, float shootCooldown, float ballSpeed)
    {
        this.ballManager = ballManager;
        this.origin = origin;
        this.shootCooldown = shootCooldown;
        this.ballSpeed = ballSpeed;

        ballPool = ballManager.ObjectPool.CreateObjectPool(
            ballPrefab,
            createFunc: () => Object.Instantiate(ballPrefab),
            onGet: ball => ball.SetActive(true),
            onRelease: ball => ball.SetActive(false));
    }

    // 인벤토리 모델(원작): 발사 간격마다 "대기 중인 볼"이 있으면 쏜다 — 회수가 발사를 만든다.
    // 전부 필드에 나가 있으면 발사하지 않고, 볼이 회수돼 대기열에 돌아오는 즉시 다음 간격에 나간다
    public void Tick(float deltaTime, Vector2 direction)
    {
        if (ballManager.CurrentGameState != GameManager.GameState.GamePlay) return;

        cooldownTimer -= deltaTime;
        if (cooldownTimer > 0f) return;

        if (ballManager.TryGetNextLoadout(out BallLoadout loadout))
        {
            Fire(loadout, direction);
            cooldownTimer = shootCooldown;   // 간격은 발사에만 걸림 — 대기 없을 땐 즉시 재시도 상태 유지
        }
    }

    private void Fire(BallLoadout loadout, Vector2 direction)
    {
        GameObject ballObj = ballPool.Get();
        ballObj.transform.position = origin.position;
        ballObj.GetComponent<SpriteRenderer>().sprite = GetSprite(loadout.spritePath);

        Ball ball = ballObj.GetComponent<Ball>();
        ball.OnHitMonster += HandleBallHitMonster;
        ball.OnExitField += HandleBallExitField;
        ball.Launch(direction, ballSpeed, origin.position, loadout);
        SoundManager.Instance?.PlaySfx(SfxClipId.BallShoot);   // null 허용 — 씬에 사운드 없는 테스트 환경 대비
    }

    // 히트는 BallManager로 중계 → SkillManager가 데미지 계산 (Ball→Shooter→Manager 중계 관례)
    private void HandleBallHitMonster(Ball ball, Collider2D monster, Vector2 hitNormal)
        => ballManager.NotifyBallHitMonster(ball, monster, hitNormal);

    // 볼은 블록에 맞아도 튕기며 계속 날아감(원작) — 회수 경로는 "바닥 반사 → 슈터 귀환" 하나뿐.
    // 회수된 볼은 인벤토리 대기열로 돌아가 먼저 회수된 순으로 재발사된다
    private void HandleBallExitField(Ball ball)
    {
        SkillId? returnedSkill = ball.ActiveSkill;
        ReleaseBall(ball);
        ballManager.ReturnBall(returnedSkill);
    }

    private void ReleaseBall(Ball ball)
    {
        ball.OnHitMonster -= HandleBallHitMonster;
        ball.OnExitField -= HandleBallExitField;
        ballPool.Release(ball.gameObject);
    }

    // 볼 스프라이트 — 경로는 로드아웃(CSV)이 들고 옴, 여기는 로드+캐시만 (스킬 종류를 모름 = OCP)
    private Sprite GetSprite(string path)
    {
        if (!spriteCache.TryGetValue(path, out Sprite sprite))
        {
            sprite = Resources.Load<Sprite>(path);
            spriteCache[path] = sprite;
        }
        return sprite;
    }

}
