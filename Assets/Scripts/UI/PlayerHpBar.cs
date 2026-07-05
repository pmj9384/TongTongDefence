using UnityEngine;

// 슈터 아래 HP 표시 (원작: 캐릭터 아래 게이지) — MVP는 숫자 텍스트 + 비율 색 변화.
// 씬 일반 오브젝트 관례: 창구 매니저(PlayerManager) 하나만 SerializeField로 참조.
public class PlayerHpBar : MonoBehaviour
{
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private float offsetY = -0.55f;

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
        text.text = current.ToString();
        text.color = Color.Lerp(new Color(0.9f, 0.25f, 0.2f), new Color(0.4f, 0.95f, 0.35f), (float)current / max);
    }

    private void Build()
    {
        var go = new GameObject("HpText");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, offsetY, 0f);

        text = go.AddComponent<TextMesh>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 52;
        text.characterSize = 0.05f;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        go.GetComponent<MeshRenderer>().sortingOrder = 5;
    }
}
