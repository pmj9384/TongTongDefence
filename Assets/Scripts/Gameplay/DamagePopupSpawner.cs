using UnityEngine;
using UnityEngine.Pool;

// 데미지 팝업의 풀+배치만 담당 (순수 C#) — MonsterSpawner/MonsterField와 같은
// "매니저 내부 컴포지션" 패턴. 팝업 오브젝트는 프리팹 없이 코드로 조립 (에셋 의존 0).
public class DamagePopupSpawner
{
    private readonly ObjectPool<DamagePopup> pool;

    public DamagePopupSpawner()
    {
        pool = new ObjectPool<DamagePopup>(
            createFunc: BuildPopup,
            actionOnGet: p => p.gameObject.SetActive(true),
            actionOnRelease: p => p.gameObject.SetActive(false));
    }

    public void Show(Vector3 position, int damage, bool isCritical)
    {
        DamagePopup popup = pool.Get();
        popup.OnFinished += HandleFinished;
        popup.Show(position, damage, isCritical);
    }

    private void HandleFinished(DamagePopup popup)
    {
        popup.OnFinished -= HandleFinished;
        pool.Release(popup);
    }

    private static DamagePopup BuildPopup()
    {
        var go = new GameObject("DamagePopup");
        TextMesh text = go.AddComponent<TextMesh>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 48;
        text.characterSize = 0.06f;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;

        var renderer = go.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = text.font.material;
        renderer.sortingOrder = 10;   // 몬스터/볼(0~1) 위

        return go.AddComponent<DamagePopup>();
    }
}
