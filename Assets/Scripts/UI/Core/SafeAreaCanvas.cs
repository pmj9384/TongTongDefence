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


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        // 에디터 제외 가드 제거 — Device Simulator가 Screen.safeArea를 제공하므로
        // 에디터에서도 적용해야 노치 잘림을 Play로 검증 가능 (시뮬레이터 미사용 시 전체화면 = 무해)
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
    }

}

