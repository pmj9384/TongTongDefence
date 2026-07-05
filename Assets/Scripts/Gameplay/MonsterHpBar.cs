using UnityEngine;

// 몬스터 체력바 — 첫 피격부터 블록 아래에 표시 (원작 관찰: HP는 숫자가 아니라 바).
// 바 스프라이트는 코드 생성 흰색 1px(에셋 의존 0), 표시만 담당 — HP 값은 Monster 이벤트 구독.
public class MonsterHpBar : MonoBehaviour
{
    private const float BarWidth = 0.9f;    // 루트 로컬 기준 (블록 = 1×1)
    private const float BarHeight = 0.1f;
    private const float BarY = -0.62f;      // 블록 바로 아래

    private static Sprite whiteSprite;

    private Monster monster;
    private GameObject barRoot;
    private Transform fill;

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
        // 왼쪽 기준으로 줄어드는 fill: 스케일 + 중심 보정
        fill.localScale = new Vector3(BarWidth * ratio, BarHeight, 1f);
        fill.localPosition = new Vector3(-BarWidth * (1f - ratio) * 0.5f, BarY, 0f);
    }

    private void Build()
    {
        barRoot = new GameObject("HpBar");
        barRoot.transform.SetParent(transform, false);

        CreateBar("Background", new Color(0.1f, 0.1f, 0.1f, 0.85f), 2,
                  new Vector3(BarWidth, BarHeight, 1f), new Vector3(0f, BarY, 0f));
        fill = CreateBar("Fill", new Color(0.35f, 0.9f, 0.3f), 3,
                  new Vector3(BarWidth, BarHeight, 1f), new Vector3(0f, BarY, 0f)).transform;

        barRoot.SetActive(false);
    }

    private GameObject CreateBar(string name, Color color, int sortingOrder, Vector3 scale, Vector3 localPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(barRoot.transform, false);
        go.transform.localScale = scale;
        go.transform.localPosition = localPos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetWhiteSprite();
        sr.color = color;
        sr.sortingOrder = sortingOrder;   // 블록(0)/몸체(1) 위
        return go;
    }

    // 1×1 흰색 스프라이트 (1월드유닛) — 색/스케일만으로 바를 그리기 위한 최소 에셋 대체물
    private static Sprite GetWhiteSprite()
    {
        if (whiteSprite != null) return whiteSprite;

        var tex = new Texture2D(4, 4);
        Color[] pixels = new Color[16];
        for (int i = 0; i < 16; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        whiteSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        return whiteSprite;
    }
}
