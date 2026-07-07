using UnityEngine;

// 몬스터 체력바 — 첫 피격부터 블록 아래에 표시 (원작 관찰: HP는 숫자가 아니라 바).
// 표시만 담당 — HP 값은 Monster 이벤트 구독. 바 조립은 Awake 1회 코드 생성:
// 다수 월드 개체라 개별 캔버스 대신 SpriteRenderer가 성능 관례 (에디터-네이티브 전환의 의도적 예외)
public class MonsterHpBar : MonoBehaviour
{
    [SerializeField] private Sprite barSprite;   // white.png (프리팹에서 연결)

    [Header("바 배치 (루트 로컬 기준, 블록=1×1) — 프리팹 Inspector에서 튜닝")]
    [SerializeField] private float barWidth = 0.9f;
    [SerializeField] private float barHeight = 0.1f;
    [SerializeField] private float barY = -0.62f;   // 블록 기준 세로 위치 (음수 = 아래)

    private const float DrainSpeed = 2.5f;   // 바가 스르륵 줄어드는 속도(비율/초) — HUD 게이지 보간과 감각 통일

    private Monster monster;
    private GameObject barRoot;
    private Transform fill;
    private SpriteRenderer fillRenderer;
    private float displayRatio = 1f;   // 표시 비율 (보간) — 실제 값은 targetRatio
    private float targetRatio = 1f;

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
        displayRatio = targetRatio = 1f;                 // 재사용 초기화 — 첫 피격 때 가득에서 스르륵
    }

    private void Refresh(int current, int max)
    {
        if (current <= 0) { barRoot.SetActive(false); return; }

        barRoot.SetActive(true);
        targetRatio = (float)current / max;   // 표시는 Update에서 보간
    }

    // 스르륵 줄어드는 연출 (HUD 게이지와 감각 통일) — 다수 개체라 Slider 대신 SpriteRenderer 유지(성능 관례)
    private void Update()
    {
        if (!barRoot.activeSelf || displayRatio == targetRatio) return;

        displayRatio = Mathf.MoveTowards(displayRatio, targetRatio, DrainSpeed * Time.deltaTime);
        fill.localScale = new Vector3(barWidth * displayRatio, barHeight, 1f);
        fill.localPosition = new Vector3(-barWidth * (1f - displayRatio) * 0.5f, barY, 0f);
    }

    private void Build()
    {
        barRoot = new GameObject("HpBar");
        barRoot.transform.SetParent(transform, false);

        CreateBar("Background", new Color(0.1f, 0.1f, 0.1f, 0.85f), 2,
                  new Vector3(barWidth, barHeight, 1f), new Vector3(0f, barY, 0f));
        GameObject fillGo = CreateBar("Fill", new Color(0.9f, 0.25f, 0.2f), 3,   // 원작: 몬스터 체력바는 빨강
                  new Vector3(barWidth, barHeight, 1f), new Vector3(0f, barY, 0f));
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
