using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Shooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BallManager ballManager;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private ShooterVisual visual;   // 조준 연출 (파츠 배치는 씬 — 미연결이어도 동작)
    [SerializeField] private Transform aimDot;       // 조준선 끝 원형 조준점 (빌더 메뉴로 생성·연결)

    [Header("Shooting")]
    [SerializeField] private float shootCooldown = 1f;
    [SerializeField] private float ballSpeed = 12f;

    [Header("Aiming")]
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
        aimer.SetAimDot(aimDot);

        inputHandler.OnDirectionChanged += dir => direction = dir;
    }

    private void Update()
    {
        if (inputHandler == null) return;

        inputHandler.Tick();
        shooter.Tick(Time.deltaTime, direction);
        aimer.Tick(direction);
        if (visual != null) visual.SetAim(direction);
    }
}
