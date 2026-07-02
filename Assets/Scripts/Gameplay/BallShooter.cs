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

    public void Tick(float deltaTime, Vector2 direction)
    {
        if (ballManager.CurrentGameState != GameManager.GameState.GamePlay) return;

        cooldownTimer -= deltaTime;
        if (cooldownTimer <= 0f)
        {
            Shoot(direction);
            cooldownTimer = shootCooldown;
        }
    }

    private void Shoot(Vector2 direction)
    {
        GameObject ballObj = ballPool.Get();
        ballObj.transform.position = origin.position;

        Ball ball = ballObj.GetComponent<Ball>();
        ball.Launch(direction, ballSpeed);
    }
}
