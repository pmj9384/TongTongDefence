using UnityEngine;

public class Shooter : MonoBehaviour
{
    [SerializeField] private BallManager ballManager;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float shootCooldown = 1f;
    [SerializeField] private float ballSpeed = 12f;
    [SerializeField] private int maxBounces = 2;
    [SerializeField] private float maxDistance = 30f;

    private BallShooter shooter;
    private ShooterAimer aimer;
    private ShooterInputHandler inputHandler;
    private Vector2 direction = Vector2.up;

    // GameManager.Awake()의 매니저 주입이 끝난 뒤여야 하므로 Awake가 아닌 Start에서 구성
    private void Start()
    {
        inputHandler = new ShooterInputHandler(transform);
        shooter = new BallShooter(ballManager, ballPrefab, transform, shootCooldown, ballSpeed);
        aimer = new ShooterAimer(ballManager, lineRenderer, transform, maxBounces, maxDistance);

        inputHandler.OnDirectionChanged += dir => direction = dir;
    }

    private void Update()
    {
        if (inputHandler == null) return;

        inputHandler.Tick();
        shooter.Tick(Time.deltaTime, direction);
        aimer.Tick(direction);
    }
}
