using UnityEngine;


public enum AnchorPresets
{
    TopLeft,
    TopCenter,
    TopRight,

    MiddleLeft,
    MiddleCenter,
    MiddleRight,

    BottomLeft,
    BottonCenter,
    BottomRight,
    BottomStretch,

    VertStretchLeft,
    VertStretchRight,
    VertStretchCenter,

    HorStretchTop,
    HorStretchMiddle,
    HorStretchBottom,

    StretchAll,
    None
}

[RequireComponent(typeof(RectTransform))]
public class SafeAreaCanvas : MonoBehaviour
{
    [HideInInspector]
    public RectTransform rectTransform;


    private Rect lastApplied;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplySafeAreaCanvasAnchor();
    }

    // 1회 적용이 아니라 변화 감지 재적용 — 씬 리로드(재시작) 직후 프레임엔 Screen.width와
    // safeArea가 다른 기준으로 읽혀 앵커가 화면 밖(3.3배)으로 계산되던 실버그의 표준 해법.
    // 잘못 계산된 프레임이 있어도 다음 프레임에 자가 교정된다 (회전/시뮬레이터 전환도 자동 대응)
    private void Update()
    {
        if (Screen.safeArea != lastApplied)
            ApplySafeAreaCanvasAnchor();
    }

    public void ApplySafeAreaCanvasAnchor()
    {
        var minAnchor = Screen.safeArea.position;
        var maxAnchor = Screen.safeArea.position + Screen.safeArea.size;

        minAnchor.x /= Screen.width;
        minAnchor.y /= Screen.height;

        maxAnchor.x /= Screen.width;
        maxAnchor.y /= Screen.height;

        rectTransform.anchorMin = minAnchor;
        rectTransform.anchorMax = maxAnchor;
        lastApplied = Screen.safeArea;
    }

}

