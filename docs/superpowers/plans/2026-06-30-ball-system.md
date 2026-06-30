# 볼 시스템 구현 플랜

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 스와이프로 방향을 조정하고 쿨다운마다 자동 발사되는 볼이 벽에 반사되며 이동하는 핀볼 시스템 구현

**Architecture:** Rigidbody2D + gravityScale=0으로 볼 이동, FixedUpdate에서 벽 경계 체크 후 velocity.x 수동 반전. ShooterInputHandler가 스와이프 방향을 BallManager에 전달하고, ShooterAimer가 Raycast로 궤적을 LineRenderer에 렌더링. BallManager는 InGameManager로 GameManager에 등록되며 SetGameManager() 주입으로 다른 시스템에 접근.

**Tech Stack:** Unity 6000.3.10, C#, Rigidbody2D, Physics2D.Raycast, LineRenderer, New Input System

## Global Constraints

- Unity 6000.3.10 / Android 타겟
- GameManager는 MonoBehaviour (싱글톤 아님) — InGameManager.SetGameManager(this) 주입 방식
- 태그 "Manager"인 오브젝트만 GameManager.RegisterManager<T>()로 자동 등록됨
- 볼 기본 데미지: 8, 치명타 확률: 0%, 치명타 데미지율: 50%
- 레이어: Ball=6, Wall=7, Monster=8 (아직 없으므로 Wall만 설정)

---

## 파일 구조

| 파일 | 역할 |
|---|---|
| `Assets/Scripts/Gameplay/BallManager.cs` | InGameManager, 쿨다운/발사/방향 관리 |
| `Assets/Scripts/Gameplay/Ball.cs` | Rigidbody2D 이동, 벽 반사, 이벤트 |
| `Assets/Scripts/Gameplay/ShooterInputHandler.cs` | 스와이프 입력 → 방향 벡터 |
| `Assets/Scripts/Gameplay/ShooterAimer.cs` | Raycast 궤적 LineRenderer |
| `Assets/Prefabs/Ball.prefab` | Ball 프리팹 (Rigidbody2D, CircleCollider2D) |
| `Assets/Scripts/Core/Managers/GameManager.cs` | BallManager 등록 추가 |

---

### Task 1: BallManager 스켈레톤 + GameManager 등록

**Files:**
- Create: `Assets/Scripts/Gameplay/BallManager.cs`
- Modify: `Assets/Scripts/Core/Managers/GameManager.cs`

**Interfaces:**
- Produces: `BallManager : InGameManager`, `GameManager.BallManager` 프로퍼티

- [ ] **Step 1: BallManager.cs 생성**

```csharp
using UnityEngine;

public class BallManager : InGameManager
{
    [SerializeField] private float shootCooldown = 1f;
    [SerializeField] private float ballSpeed = 12f;

    private ShooterInputHandler inputHandler;
    private ShooterAimer aimer;
    private Vector2 shootDirection = Vector2.up;

    private void Awake()
    {
        inputHandler = GetComponent<ShooterInputHandler>();
        aimer = GetComponent<ShooterAimer>();
    }

    public override void Initialize()
    {
        base.Initialize();
    }
}
```

- [ ] **Step 2: GameManager에 BallManager 등록**

`Assets/Scripts/Core/Managers/GameManager.cs` 수정:

```csharp
public BallManager BallManager { get; private set; }
```

`InitializeCoreManagers()` 안에 추가:
```csharp
BallManager = RegisterManager<BallManager>(managerObjects);
```

- [ ] **Step 3: 씬에 BallManager 오브젝트 생성 (MCP)**

씬에 `BallManager` 오브젝트 생성, 태그 `Manager` 설정, `BallManager` 컴포넌트 추가.

- [ ] **Step 4: Play Mode 확인**

Play Mode 진입 → Console에 에러 없음, BallManager.Initialize() 호출됨 확인.

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/Gameplay/BallManager.cs Assets/Scripts/Core/Managers/GameManager.cs
git commit -m "feat: BallManager 스켈레톤 + GameManager 등록"
```

---

### Task 2: ShooterInputHandler — 스와이프 입력

**Files:**
- Create: `Assets/Scripts/Gameplay/ShooterInputHandler.cs`

**Interfaces:**
- Produces: `ShooterInputHandler.OnDirectionChanged : event Action<Vector2>`

- [ ] **Step 1: ShooterInputHandler.cs 생성**

```csharp
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShooterInputHandler : MonoBehaviour
{
    public event Action<Vector2> OnDirectionChanged;

    private Vector2 touchStartPos;
    private bool isSwiping;

    private void Update()
    {
        if (Touchscreen.current != null)
            HandleTouch();
        else
            HandleMouse();
    }

    private void HandleTouch()
    {
        var touch = Touchscreen.current.primaryTouch;
        if (touch.press.wasPressedThisFrame)
        {
            touchStartPos = touch.position.ReadValue();
            isSwiping = true;
        }
        if (isSwiping && touch.press.isPressed)
        {
            Vector2 delta = touch.position.ReadValue() - touchStartPos;
            if (delta.magnitude > 10f)
                FireDirection(delta);
        }
        if (touch.press.wasReleasedThisFrame)
            isSwiping = false;
    }

    private void HandleMouse()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            touchStartPos = Mouse.current.position.ReadValue();
            isSwiping = true;
        }
        if (isSwiping && Mouse.current.leftButton.isPressed)
        {
            Vector2 delta = Mouse.current.position.ReadValue() - touchStartPos;
            if (delta.magnitude > 10f)
                FireDirection(delta);
        }
        if (Mouse.current.leftButton.wasReleasedThisFrame)
            isSwiping = false;
    }

    private void FireDirection(Vector2 delta)
    {
        Vector2 dir = delta.normalized;
        // 아래 방향 방지 (y > 0만 허용)
        if (dir.y < 0.1f)
            dir = new Vector2(dir.x, 0.1f).normalized;
        OnDirectionChanged?.Invoke(dir);
    }
}
```

- [ ] **Step 2: BallManager에서 구독**

`BallManager.Awake()`에 추가:
```csharp
if (inputHandler != null)
    inputHandler.OnDirectionChanged += dir => shootDirection = dir;
```

- [ ] **Step 3: BallManager 오브젝트에 ShooterInputHandler 컴포넌트 추가 (MCP)**

- [ ] **Step 4: Play Mode 확인**

Play Mode → 마우스 드래그 시 `shootDirection` 값이 변하는지 Debug.Log로 확인.

일시적으로 BallManager.Update()에 추가:
```csharp
private void Update() => Debug.Log($"Direction: {shootDirection}");
```

확인 후 제거.

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/Gameplay/ShooterInputHandler.cs Assets/Scripts/Gameplay/BallManager.cs
git commit -m "feat: ShooterInputHandler 스와이프 입력 구현"
```

---

### Task 3: Ball — Rigidbody2D 이동 + 벽 반사

**Files:**
- Create: `Assets/Scripts/Gameplay/Ball.cs`
- Create: `Assets/Prefabs/Ball.prefab`

**Interfaces:**
- Consumes: 벽 경계값 (BallManager에서 Launch() 호출 시 전달)
- Produces:
  - `Ball.Launch(Vector2 direction, float speed, float leftWall, float rightWall)`
  - `Ball.OnHitWall : event Action<Ball>`
  - `Ball.OnHitMonster : event Action<Ball, Collider2D>`

- [ ] **Step 1: Ball.cs 생성**

```csharp
using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Ball : MonoBehaviour
{
    public event Action<Ball> OnHitWall;
    public event Action<Ball, Collider2D> OnHitMonster;

    public int WallBounceCount { get; private set; }

    private Rigidbody2D rb;
    private float leftWall;
    private float rightWall;
    private float topWall;

    public void Launch(Vector2 direction, float speed, float leftWall, float rightWall, float topWall)
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = direction.normalized * speed;

        this.leftWall = leftWall;
        this.rightWall = rightWall;
        this.topWall = topWall;
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        float x = rb.position.x;
        float y = rb.position.y;

        if (x <= leftWall)
        {
            rb.position = new Vector2(leftWall, rb.position.y);
            rb.linearVelocity = new Vector2(Mathf.Abs(rb.linearVelocity.x), rb.linearVelocity.y);
            WallBounceCount++;
            OnHitWall?.Invoke(this);
        }
        else if (x >= rightWall)
        {
            rb.position = new Vector2(rightWall, rb.position.y);
            rb.linearVelocity = new Vector2(-Mathf.Abs(rb.linearVelocity.x), rb.linearVelocity.y);
            WallBounceCount++;
            OnHitWall?.Invoke(this);
        }

        if (y >= topWall)
        {
            rb.position = new Vector2(rb.position.x, topWall);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -Mathf.Abs(rb.linearVelocity.y));
            WallBounceCount++;
            OnHitWall?.Invoke(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
            OnHitMonster?.Invoke(this, other);
    }
}
```

- [ ] **Step 2: Ball 프리팹 생성**

Unity에서:
1. Hierarchy에 빈 오브젝트 생성 → 이름 `Ball`
2. 컴포넌트 추가: `Ball`, `Rigidbody2D`(gravityScale=0, Collision Detection=Continuous), `CircleCollider2D`(isTrigger=true, radius=0.2)
3. SpriteRenderer 추가 (임시 흰 원 스프라이트)
4. `Assets/Prefabs/Ball.prefab`으로 저장 후 씬에서 삭제

- [ ] **Step 3: Play Mode 확인**

임시 테스트 코드를 BallManager.Start()에 추가:
```csharp
private void Start()
{
    var go = Instantiate(ballPrefab, Vector3.zero, Quaternion.identity);
    var ball = go.GetComponent<Ball>();
    ball.OnHitWall += b => Debug.Log($"Hit wall! Bounce: {b.WallBounceCount}");
    ball.Launch(Vector2.up + Vector2.right, 8f, -2.5f, 2.5f, 4f);
}
```

Play Mode → 볼이 이동하고 벽에서 반사되는지 확인. 확인 후 임시 코드 제거.

- [ ] **Step 4: 커밋**

```bash
git add Assets/Scripts/Gameplay/Ball.cs Assets/Prefabs/Ball.prefab Assets/Prefabs/Ball.prefab.meta
git commit -m "feat: Ball Rigidbody2D 이동 + 벽 반사 구현"
```

---

### Task 4: ShooterAimer — Raycast 궤적 LineRenderer

**Files:**
- Create: `Assets/Scripts/Gameplay/ShooterAimer.cs`

**Interfaces:**
- Consumes: `UpdateAim(Vector2 origin, Vector2 direction, float leftWall, float rightWall, float topWall)`
- Produces: LineRenderer로 궤적 시각화

- [ ] **Step 1: ShooterAimer.cs 생성**

```csharp
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ShooterAimer : MonoBehaviour
{
    [SerializeField] private int maxBounces = 5;
    [SerializeField] private float maxDistance = 30f;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 0;
    }

    public void UpdateAim(Vector2 origin, Vector2 direction, float leftWall, float rightWall, float topWall)
    {
        var points = CalculateTrajectory(origin, direction, leftWall, rightWall, topWall);
        lineRenderer.positionCount = points.Length;
        for (int i = 0; i < points.Length; i++)
            lineRenderer.SetPosition(i, points[i]);
    }

    public void HideAim() => lineRenderer.positionCount = 0;

    private Vector3[] CalculateTrajectory(Vector2 origin, Vector2 dir, float leftWall, float rightWall, float topWall)
    {
        var points = new System.Collections.Generic.List<Vector3>();
        points.Add(origin);

        Vector2 pos = origin;
        Vector2 vel = dir.normalized;
        float remaining = maxDistance;

        for (int bounce = 0; bounce < maxBounces && remaining > 0; bounce++)
        {
            // 다음 벽까지 거리 계산
            float distToLeft   = vel.x < 0 ? (leftWall  - pos.x) / vel.x : float.MaxValue;
            float distToRight  = vel.x > 0 ? (rightWall - pos.x) / vel.x : float.MaxValue;
            float distToTop    = vel.y > 0 ? (topWall   - pos.y) / vel.y : float.MaxValue;

            // 몬스터 Raycast
            var hit = Physics2D.Raycast(pos, vel, remaining, LayerMask.GetMask("Monster"));
            float distToMonster = hit.collider != null ? hit.distance : float.MaxValue;

            float minDist = Mathf.Min(distToLeft, distToRight, distToTop, distToMonster, remaining);

            pos += vel * minDist;
            points.Add(pos);
            remaining -= minDist;

            if (minDist == distToMonster) break;

            // 반사
            if (minDist == distToLeft || minDist == distToRight)
                vel.x = -vel.x;
            if (minDist == distToTop)
                vel.y = -vel.y;
        }

        return points.ToArray();
    }
}
```

- [ ] **Step 2: BallManager 오브젝트에 LineRenderer + ShooterAimer 추가 (MCP)**

BallManager 오브젝트에 `LineRenderer`, `ShooterAimer` 컴포넌트 추가.

- [ ] **Step 3: BallManager에서 UpdateAim 호출**

`BallManager.cs`에 Update 추가:
```csharp
[SerializeField] private Transform shootOrigin; // 발사 위치
private float leftWall, rightWall, topWall;

public override void Initialize()
{
    base.Initialize();
    // 카메라 기준 경계 계산
    Camera cam = Camera.main;
    float camZ = Mathf.Abs(cam.transform.position.z);
    Vector3 bottomLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, camZ));
    Vector3 topRight   = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, camZ));
    leftWall  = bottomLeft.x;
    rightWall = topRight.x;
    topWall   = topRight.y;
}

private void Update()
{
    if (aimer != null && shootOrigin != null)
        aimer.UpdateAim(shootOrigin.position, shootDirection, leftWall, rightWall, topWall);
}
```

씬에서 BallManager 오브젝트 하위에 빈 오브젝트 `ShootOrigin` 생성 후 shootOrigin에 연결.

- [ ] **Step 4: Play Mode 확인**

Play Mode → 마우스 드래그 시 흰 라인이 벽에 튕기며 그려지는지 확인.

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/Gameplay/ShooterAimer.cs Assets/Scripts/Gameplay/BallManager.cs
git commit -m "feat: ShooterAimer Raycast 궤적 LineRenderer 구현"
```

---

### Task 5: BallManager — 쿨다운 자동 발사

**Files:**
- Modify: `Assets/Scripts/Gameplay/BallManager.cs`

**Interfaces:**
- Consumes: `Ball.prefab`, `ShooterInputHandler.OnDirectionChanged`, 경계값
- Produces: 쿨다운마다 Ball 자동 생성 + Launch

- [ ] **Step 1: BallManager에 발사 로직 추가**

```csharp
[SerializeField] private GameObject ballPrefab;

private float cooldownTimer;

// Update()에 추가:
private void Update()
{
    if (GameManager.CurrentState != GameManager.GameState.GamePlay) return;

    aimer?.UpdateAim(shootOrigin.position, shootDirection, leftWall, rightWall, topWall);

    cooldownTimer -= Time.deltaTime;
    if (cooldownTimer <= 0f)
    {
        Shoot();
        cooldownTimer = shootCooldown;
    }
}

private void Shoot()
{
    var go = Instantiate(ballPrefab, shootOrigin.position, Quaternion.identity);
    var ball = go.GetComponent<Ball>();
    ball.OnHitWall += OnBallHitWall;
    ball.OnHitMonster += OnBallHitMonster;
    ball.Launch(shootDirection, ballSpeed, leftWall, rightWall, topWall);
}

private void OnBallHitWall(Ball ball)
{
    // 패시브 스킬 처리 예정 (마법 거울 등)
}

private void OnBallHitMonster(Ball ball, Collider2D monster)
{
    // 데미지 처리 예정
    Destroy(ball.gameObject);
}
```

- [ ] **Step 2: Ball 프리팹 Inspector 연결**

BallManager Inspector → `ballPrefab` 필드에 `Assets/Prefabs/Ball.prefab` 연결.

- [ ] **Step 3: Play Mode 확인**

1. GameState를 GamePlay로 수동 설정 또는 GameManager.Start()에서 자동 진입 확인
2. 쿨다운마다 볼이 발사되고 벽에서 반사되는지 확인
3. 스와이프로 방향이 바뀌고 궤적 라인도 따라 바뀌는지 확인

- [ ] **Step 4: 커밋**

```bash
git add Assets/Scripts/Gameplay/BallManager.cs
git commit -m "feat: BallManager 쿨다운 자동 발사 구현"
```
