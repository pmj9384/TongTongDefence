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
    }
}
