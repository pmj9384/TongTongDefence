using UnityEngine;

public class FieldManager : InGameManager
{
    public float LeftWall { get; private set; }
    public float RightWall { get; private set; }
    public float TopWall { get; private set; }
    public float BottomWall { get; private set; }

    [SerializeField] private SpriteRenderer backgroundSprite;
    // 배경 스프라이트 안에서 "페인팅된 격자 영역"이 차지하는 정규화 사각형 (0~1).
    // Background_1_Stage.png 픽셀 분석으로 실측(좌408,상483,우1607,하1630 / 2048px 기준).
    [SerializeField] private Rect gridRegionNormalized = new Rect(0.1992f, 0.2041f, 0.5854f, 0.5601f);

    public override void Initialize()
    {
        base.Initialize();
        CalculateFieldBounds();
        CreateWallColliders();
    }

    private void CalculateFieldBounds()
    {
        Bounds b = backgroundSprite.bounds; // 월드 공간, 배경 스케일 반영됨
        LeftWall   = b.min.x + gridRegionNormalized.xMin * b.size.x;
        RightWall  = b.min.x + gridRegionNormalized.xMax * b.size.x;
        BottomWall = b.min.y + gridRegionNormalized.yMin * b.size.y;
        TopWall    = b.min.y + gridRegionNormalized.yMax * b.size.y;
    }

    private void CreateWallColliders()
    {
        int wallLayer = LayerMask.NameToLayer("Wall");
        float thickness = 1f;
        float height = TopWall - BottomWall;
        float width = RightWall - LeftWall;

        CreateWall("LeftWall", new Vector2(LeftWall - thickness / 2f, (TopWall + BottomWall) / 2f), new Vector2(thickness, height), wallLayer, isTrigger: false);
        CreateWall("RightWall", new Vector2(RightWall + thickness / 2f, (TopWall + BottomWall) / 2f), new Vector2(thickness, height), wallLayer, isTrigger: false);
        CreateWall("TopWall", new Vector2((LeftWall + RightWall) / 2f, TopWall + thickness / 2f), new Vector2(width + thickness * 2f, thickness), wallLayer, isTrigger: false);
        // BottomWall은 물리 반사 없이 "필드 이탈" 감지만 — 볼이 아무것도 못 맞히고 빠져나가면 풀로 반환(Ball.OnExitField)
        CreateWall("BottomWall", new Vector2((LeftWall + RightWall) / 2f, BottomWall - thickness / 2f), new Vector2(width + thickness * 2f, thickness), wallLayer, isTrigger: true);
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
