using UnityEngine;
using UnityEngine.Pool;

public class BallShooter : MonoBehaviour
{
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform shootOrigin;
    [SerializeField] private float shootCooldown = 1f;
    [SerializeField] private float ballSpeed = 12f;

    private BallManager ballManager;
    private ShooterInputHandler inputHandler;
    private ObjectPool<GameObject> ballPool;

    private Vector2 shootDirection = Vector2.up;
    private float cooldownTimer;

    private void Awake()
    {
        ballManager = GetComponent<BallManager>();
        inputHandler = GetComponent<ShooterInputHandler>();
        inputHandler.OnDirectionChanged += dir => shootDirection = dir;
    }

    private void Start()
    {
        ballPool = ballManager.ObjectPool.CreateObjectPool(
            ballPrefab,
            createFunc: () => Instantiate(ballPrefab),
            onGet: ball => ball.SetActive(true),
            onRelease: ball => ball.SetActive(false));
    }

    private void Update()
    {
        if (ballManager.CurrentGameState != GameManager.GameState.GamePlay) return;

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f)
        {
            Shoot();
            cooldownTimer = shootCooldown;
        }
    }

    private void Shoot()
    {
        GameObject ballObj = ballPool.Get();
        ballObj.transform.position = shootOrigin.position;

        Ball ball = ballObj.GetComponent<Ball>();
        ball.Launch(shootDirection, ballSpeed, ballManager.LeftWall, ballManager.RightWall, ballManager.TopWall);
    }
}
