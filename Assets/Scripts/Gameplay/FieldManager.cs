using UnityEngine;

public class FieldManager : InGameManager
{
    public float LeftWall { get; private set; }
    public float RightWall { get; private set; }
    public float TopWall { get; private set; }
    public float BottomWall { get; private set; }

    public override void Initialize()
    {
        base.Initialize();
        Camera cam = Camera.main;
        float camZ = Mathf.Abs(cam.transform.position.z);

        Vector3 bottomLeft = cam.ScreenToWorldPoint(new Vector3(Screen.safeArea.xMin, Screen.safeArea.yMin, camZ));
        Vector3 topRight = cam.ScreenToWorldPoint(new Vector3(Screen.safeArea.xMax, Screen.safeArea.yMax, camZ));

        LeftWall = bottomLeft.x;
        RightWall = topRight.x;
        BottomWall = bottomLeft.y;
        TopWall = topRight.y;

        CreateWallColliders();
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
