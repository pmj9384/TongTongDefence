using UnityEngine;

// 슈터 아래 플레이어 체력 게이지 (원작: 녹색 바 + 숫자) — 바 + 바 안 흰색 숫자.
// 씬 일반 오브젝트 관례: 창구 매니저(PlayerManager) 하나만 SerializeField로 참조.
public class PlayerHpBar : MonoBehaviour
{
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private float offsetY = -0.55f;

    private const float BarWidth = 1.3f;
    private const float BarHeight = 0.22f;

    private Transform fill;
    private SpriteRenderer fillRenderer;
    private TextMesh text;

    private void Start()   // 매니저 Initialize 완료 후 (Shooter와 동일한 이유)
    {
        Build();
        playerManager.Health.OnChanged += Refresh;
        Refresh(playerManager.Health.Current, playerManager.Health.Max);
    }

    private void OnDestroy()
    {
        if (playerManager != null && playerManager.Health != null)
            playerManager.Health.OnChanged -= Refresh;
    }

    private void Refresh(int current, int max)
    {
        float ratio = (float)current / max;
        fill.localScale = new Vector3(BarWidth * ratio, BarHeight, 1f);
        fill.localPosition = new Vector3(-BarWidth * (1f - ratio) * 0.5f, offsetY, 0f);
        text.text = current.ToString();   // 색은 초록 고정 (원작 — 그라데이션 없음)
    }

    private void Build()
    {
        CreateBar("HpBarBackground", new Color(0.08f, 0.08f, 0.08f, 0.9f), 5);
        GameObject fillGo = CreateBar("HpBarFill", new Color(0.35f, 0.9f, 0.3f), 6);
        fill = fillGo.transform;
        fillRenderer = fillGo.GetComponent<SpriteRenderer>();

        // 바 안의 숫자 — 흰색, 바 위에 렌더
        var textGo = new GameObject("HpText");
        textGo.transform.SetParent(transform, false);
        textGo.transform.localPosition = new Vector3(0f, offsetY, 0f);
        text = textGo.AddComponent<TextMesh>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 48;
        text.characterSize = 0.045f;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = Color.white;
        textGo.GetComponent<MeshRenderer>().sortingOrder = 7;
    }

    private GameObject CreateBar(string name, Color color, int sortingOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localScale = new Vector3(BarWidth, BarHeight, 1f);
        go.transform.localPosition = new Vector3(0f, offsetY, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = RuntimeSprites.White;
        sr.color = color;
        sr.sortingOrder = sortingOrder;
        return go;
    }
}
