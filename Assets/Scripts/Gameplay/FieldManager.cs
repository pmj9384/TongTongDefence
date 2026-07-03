using UnityEngine;

public class FieldManager : InGameManager
{
    public float LeftWall { get; private set; }
    public float RightWall { get; private set; }
    public float TopWall { get; private set; }
    public float BottomWall { get; private set; }

    [SerializeField] private SpriteRenderer backgroundSprite;
    // 화면(뷰포트 0~1)에서 플레이필드가 차지하는 사각형 — 필드 좌표의 SSOT.
    // 배경이 화면에 1:1 stretch되므로 "그림 속 돌판의 정규화 좌표 = 뷰포트 좌표".
    // 돌판 픽셀 실측(좌408,상483,우1607,하1630 / 2048) — 아트가 정해주는 값이라 임의 튜닝 불필요.
    // 격자·몬스터·벽이 그림과 태생적으로 일치 (원본 게임의 '아트=로직 좌표' 방식을 이 에셋에 적용).
    [SerializeField] private Rect fieldRegionViewport = new Rect(0.1992f, 0.2041f, 0.5854f, 0.5601f);

    public override void Initialize()
    {
        base.Initialize();
        Camera cam = Camera.main;
        float camZ = Mathf.Abs(cam.transform.position.z);

        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(fieldRegionViewport.xMin, fieldRegionViewport.yMin, camZ));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(fieldRegionViewport.xMax, fieldRegionViewport.yMax, camZ));
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
