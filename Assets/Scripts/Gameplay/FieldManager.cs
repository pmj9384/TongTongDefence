using UnityEngine;

public class FieldManager : InGameManager
{
    public float LeftWall { get; private set; }
    public float RightWall { get; private set; }
    public float TopWall { get; private set; }
    public float BottomWall { get; private set; }

    [SerializeField] private SpriteRenderer backgroundSprite;
    // 필드 = 디자인 결정값 (화면 뷰포트 비율): 중앙 정렬 폭 90%, 아래 18% 슈터 밴드, 위 20% UI 밴드.
    // 그림 격자선과의 픽셀 정렬은 공식 폐기 — 선은 흐릿한 장식이고(게임 중 인지 불가),
    // 칸의 시각화는 블록 시스템(몬스터 발밑 블록)이 담당한다. 계측 노선(boardPixelRect)은
    // 오차·재측정 루프를 낳아 폐기함. 로직이 기준, 비주얼이 따라온다.
    // 값은 Scene 뷰의 격자 Gizmo를 보며 눈으로 튜닝 (2026-07-04, 유저 확정).
    // 오른쪽 여백(0.07)이 왼쪽(0.13)보다 좁은 비대칭 — 그림의 판이 화면 중앙에서 살짝 오른쪽이라 의도된 값.
    [SerializeField] private Rect fieldViewportRect = new Rect(0.13f, 0.218f, 0.80f, 0.585f);

    // 논리 격자 — 필드를 columns×rows 셀로 나눈다. 모든 시스템이 (행, 열) 주소로 위치를 지목:
    // 스폰 대형, 블록 점유(1×2 등), 레이저볼 "같은 행" 판정의 공통 기반.
    [SerializeField] private int columns = 9;
    [SerializeField] private int rows = 13;

    public int Columns => columns;
    public int Rows => rows;
    public float CellWidth => (RightWall - LeftWall) / columns;
    public float CellHeight => (TopWall - BottomWall) / rows;

    // row 0 = 맨 윗줄(스폰줄), col 0 = 왼쪽. 셀의 "중심" 월드 좌표를 반환.
    public Vector2 CellToWorld(int row, int col)
        => new Vector2(LeftWall + (col + 0.5f) * CellWidth,
                       TopWall - (row + 0.5f) * CellHeight);

    public override void Initialize()
    {
        base.Initialize();
        Camera cam = Camera.main;
        float camZ = Mathf.Abs(cam.transform.position.z);

        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(fieldViewportRect.xMin, fieldViewportRect.yMin, camZ));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(fieldViewportRect.xMax, fieldViewportRect.yMax, camZ));
        LeftWall = bottomLeft.x;
        RightWall = topRight.x;
        BottomWall = bottomLeft.y;
        TopWall = topRight.y;

        FitBackgroundToCamera(cam, camZ);
        CreateWallColliders();
    }

    // 배경을 화면(카메라 rect)에 정확히 맞춤 — 가로/세로 독립 배율(stretch fill).
    // 지급 리소스가 세로 원화를 2048² 정사각으로 압축해 담은 텍스처라 세로로 펴는 것이 원본 비율 복원.
    private void FitBackgroundToCamera(Camera cam, float camZ)
    {
        Vector3 screenBottomLeft = cam.ViewportToWorldPoint(new Vector3(0f, 0f, camZ));
        Vector3 screenTopRight = cam.ViewportToWorldPoint(new Vector3(1f, 1f, camZ));
        float screenWidth = screenTopRight.x - screenBottomLeft.x;
        float screenHeight = screenTopRight.y - screenBottomLeft.y;

        backgroundSprite.drawMode = SpriteDrawMode.Simple;
        backgroundSprite.sortingOrder = -10; // 보드(-9)·몬스터/볼(0)보다 뒤
        Vector2 baseSize = backgroundSprite.sprite.bounds.size; // 스케일 1 기준 크기
        backgroundSprite.transform.localScale = new Vector3(
            screenWidth / baseSize.x, screenHeight / baseSize.y, 1f);
        backgroundSprite.transform.position = new Vector3(
            (screenBottomLeft.x + screenTopRight.x) * 0.5f,
            (screenBottomLeft.y + screenTopRight.y) * 0.5f,
            backgroundSprite.transform.position.z);
    }

    // 볼 반사 바닥을 판 바닥보다 얼마나 아래(뿌리지대) 둘지 — 슈터가 이 사이에 서 있어야
    // "볼이 슈터를 지나 내려가 → 바닥에 튕겨 → 슈터에게 돌아오는" 원작 회수 연출이 나온다
    [SerializeField] private float bottomWallDrop = 0.5f;
    // 반사 벽(좌/우/상)을 판 가장자리보다 바깥으로 미는 여유 — 반사 공간 확보용.
    // 격자/몬스터 좌표는 불변, 벽만 밀림. Play 보며 인스펙터 튜닝.
    [SerializeField] private float wallPadding = 0.15f;

    private void CreateWallColliders()
    {
        int wallLayer = LayerMask.NameToLayer("Wall");
        float thickness = 1f;
        float left = LeftWall - wallPadding;
        float right = RightWall + wallPadding;
        float top = TopWall + wallPadding;
        float bounceFloorY = BottomWall - bottomWallDrop;
        float height = top - bounceFloorY; // 측벽도 반사 바닥까지 내려 필드를 완전한 사각형으로 밀폐
        float width = right - left;

        CreateWall("LeftWall", new Vector2(left - thickness / 2f, (top + bounceFloorY) / 2f), new Vector2(thickness, height), wallLayer, isTrigger: false);
        CreateWall("RightWall", new Vector2(right + thickness / 2f, (top + bounceFloorY) / 2f), new Vector2(thickness, height), wallLayer, isTrigger: false);
        CreateWall("TopWall", new Vector2((left + right) / 2f, top + thickness / 2f), new Vector2(width + thickness * 2f, thickness), wallLayer, isTrigger: false);
        // 하단도 물리 벽 — 볼이 한 번 튕기면 회수 모드로 전환되어 슈터에게 돌아감 (Ball.OnCollisionEnter2D의 노멀 판정)
        CreateWall("BottomWall", new Vector2((left + right) / 2f, bounceFloorY - thickness / 2f), new Vector2(width + thickness * 2f, thickness), wallLayer, isTrigger: false);
    }

    private void CreateWall(string name, Vector2 position, Vector2 size, int layer, bool isTrigger)
    {
        GameObject wall = new GameObject(name);
        wall.transform.SetParent(transform);
        wall.transform.position = position;
        wall.layer = layer;
        BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
        collider.size = size;
        collider.isTrigger = isTrigger;
    }

#if UNITY_EDITOR
    // Scene 뷰에 논리 격자(9×13)를 초록 선으로 그린다 — 몬스터/그림/필드를 눈으로 비교하는 진단 도구.
    // 에디터 전용(빌드 제외), Play 중이 아니어도 보임 (fieldViewportRect에서 즉석 계산)
    private void OnDrawGizmos()
    {
        Camera cam = Camera.main;
        if (cam == null || columns <= 0 || rows <= 0) return;
        float camZ = Mathf.Abs(cam.transform.position.z);
        Vector3 bl = cam.ViewportToWorldPoint(new Vector3(fieldViewportRect.xMin, fieldViewportRect.yMin, camZ));
        Vector3 tr = cam.ViewportToWorldPoint(new Vector3(fieldViewportRect.xMax, fieldViewportRect.yMax, camZ));
        float cellW = (tr.x - bl.x) / columns;
        float cellH = (tr.y - bl.y) / rows;

        Gizmos.color = new Color(0f, 1f, 0.4f, 0.9f);
        for (int c = 0; c <= columns; c++)
        {
            float x = bl.x + c * cellW;
            Gizmos.DrawLine(new Vector3(x, bl.y, 0f), new Vector3(x, tr.y, 0f));
        }
        for (int r = 0; r <= rows; r++)
        {
            float y = bl.y + r * cellH;
            Gizmos.DrawLine(new Vector3(bl.x, y, 0f), new Vector3(tr.x, y, 0f));
        }
    }
#endif
}
