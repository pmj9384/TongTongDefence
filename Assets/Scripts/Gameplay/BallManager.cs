using UnityEngine;

public class BallManager : InGameManager
{
    public float LeftWall { get; private set; }
    public float RightWall { get; private set; }
    public float TopWall { get; private set; }

    public override void Initialize()
    {
        base.Initialize();
        CalculateFieldBounds();
        Debug.Log($"[BallManager] FieldBounds: left={LeftWall}, right={RightWall}, top={TopWall}");
    }

    private void CalculateFieldBounds()
    {
        Camera cam = Camera.main;
        float camZ = Mathf.Abs(cam.transform.position.z);
        Vector3 bottomLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, camZ));
        Vector3 topRight = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, camZ));

        LeftWall = bottomLeft.x;
        RightWall = topRight.x;
        TopWall = topRight.y;
    }
}
