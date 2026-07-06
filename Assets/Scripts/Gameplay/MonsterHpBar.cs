using UnityEngine;

// 몬스터 체력바 — 첫 피격부터 블록 아래에 표시 (원작 관찰: HP는 숫자가 아니라 바).
// 표시만 담당 — HP 값은 Monster 이벤트 구독. 바 조립은 Awake 1회 코드 생성:
// 다수 월드 개체라 개별 캔버스 대신 SpriteRenderer가 성능 관례 (에디터-네이티브 전환의 의도적 예외)
public class MonsterHpBar : MonoBehaviour
{
    [SerializeField] private Sprite barSprite;   // white.png (프리팹에서 연결)

    private const float BarWidth = 0.9f;    // 루트 로컬 기준 (블록 = 1×1)
    private const float BarHeight = 0.1f;
    private const float BarY = -0.62f;      // 블록 바로 아래

    private Monster monster;
    private GameObject barRoot;
    private Transform fill;
    private SpriteRenderer fillRenderer;

    private void Awake()
    {
        monster = GetComponent<Monster>();
        monster.OnHpChanged += Refresh;
        Build();
    }

    private void OnDestroy()
    {
        if (monster != null) monster.OnHpChanged -= Refresh;
    }

    private void OnEnable()
    {
        if (barRoot != null) barRoot.SetActive(false);   // 풀 재사용 — 피격 전엔 숨김
    }

    private void Refresh(int current, int max)
    {
        if (current <= 0) { barRoot.SetActive(false); return; }

        barRoot.SetActive(true);
        float ratio = (float)current / max;
        // 왼쪽 기준으로 줄어드는 fill: 스케일 + 중심 보정. 색은 고정 빨강 (원작 관찰)
        fill.localScale = new Vector3(BarWidth * ratio, BarHeight, 1f);
        fill.localPosition = new Vector3(-BarWidth * (1f - ratio) * 0.5f, BarY, 0f);
    }

    private void Build()
    {
        barRoot = new GameObject("HpBar");
        barRoot.transform.SetParent(transform, false);

        CreateBar("Background", new Color(0.1f, 0.1f, 0.1f, 0.85f), 2,
                  new Vector3(BarWidth, BarHeight, 1f), new Vector3(0f, BarY, 0f));
        GameObject fillGo = CreateBar("Fill", new Color(0.9f, 0.25f, 0.2f), 3,   // 원작: 몬스터 체력바는 빨강
                  new Vector3(BarWidth, BarHeight, 1f), new Vector3(0f, BarY, 0f));
        fill = fillGo.transform;
        fillRenderer = fillGo.GetComponent<SpriteRenderer>();

        barRoot.SetActive(false);
    }

    private GameObject CreateBar(string name, Color color, int sortingOrder, Vector3 scale, Vector3 localPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(barRoot.transform, false);
        go.transform.localScale = scale;
        go.transform.localPosition = localPos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = barSprite;
        sr.color = color;
        sr.sortingOrder = sortingOrder;   // 블록(0)/몸체(1) 위
        return go;
    }
}
