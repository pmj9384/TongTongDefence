using UnityEngine;

public class FieldManager : InGameManager
{
    public float LeftWall { get; private set; }
    public float RightWall { get; private set; }
    public float TopWall { get; private set; }
    public float BottomWall { get; private set; }

    [SerializeField] private SpriteRenderer backgroundSprite;
    // 그림 속 돌판의 위치 — "텍스처 픽셀" 좌표(하단 원점). 원본 2048² 기준 실측: 좌408, 하418, 폭1199, 높1147.
    // 그림에 대한 사실이라 불변. 스프라이트 rect(Sprite Editor에서 다듬은 표시 영역)가 바뀌어도
    // Initialize가 현재 rect 기준으로 자동 환산하므로 필드가 항상 판을 따라간다.
    [SerializeField] private Rect boardPixelRect = new Rect(408f, 418f, 1199f, 1147f);

    public override void Initialize()
    {
        base.Initialize();
        Camera cam = Camera.main;
        float camZ = Mathf.Abs(cam.transform.position.z);

        // 배경이 화면에 1:1 stretch되므로 "스프라이트 표시영역 내 정규화 좌표 = 뷰포트 좌표".
        // 판 픽셀 좌표를 현재 스프라이트 rect 기준으로 정규화해 환산 (rect가 잘려 있어도 정확)
        Rect spriteRect = backgroundSprite.sprite.rect;
        float xMin = (boardPixelRect.xMin - spriteRect.xMin) / spriteRect.width;
        float xMax = (boardPixelRect.xMax - spriteRect.xMin) / spriteRect.width;
        float yMin = (boardPixelRect.yMin - spriteRect.yMin) / spriteRect.height;
        float yMax = (boardPixelRect.yMax - spriteRect.yMin) / spriteRect.height;

        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(xMin, yMin, camZ));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(xMax, yMax, camZ));
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

    private void CreateWallColliders()
    {
        int wallLayer = LayerMask.NameToLayer("Wall");
        float thickness = 1f;
        float bounceFloorY = BottomWall - bottomWallDrop;
        float height = TopWall - bounceFloorY; // 측벽도 반사 바닥까지 내려 필드를 완전한 사각형으로 밀폐
        float width = RightWall - LeftWall;

        CreateWall("LeftWall", new Vector2(LeftWall - thickness / 2f, (TopWall + bounceFloorY) / 2f), new Vector2(thickness, height), wallLayer, isTrigger: false);
        CreateWall("RightWall", new Vector2(RightWall + thickness / 2f, (TopWall + bounceFloorY) / 2f), new Vector2(thickness, height), wallLayer, isTrigger: false);
        CreateWall("TopWall", new Vector2((LeftWall + RightWall) / 2f, TopWall + thickness / 2f), new Vector2(width + thickness * 2f, thickness), wallLayer, isTrigger: false);
        // 하단도 물리 벽 — 볼이 한 번 튕기면 회수 모드로 전환되어 슈터에게 돌아감 (Ball.OnCollisionEnter2D의 노멀 판정)
        CreateWall("BottomWall", new Vector2((LeftWall + RightWall) / 2f, bounceFloorY - thickness / 2f), new Vector2(width + thickness * 2f, thickness), wallLayer, isTrigger: false);
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
}
