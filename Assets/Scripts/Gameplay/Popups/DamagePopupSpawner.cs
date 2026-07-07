using UnityEngine;
using UnityEngine.Pool;

// 데미지 팝업의 풀+배치만 담당 (순수 C#) — MonsterSpawner/FragmentBall 풀과 동일하게
// ObjectPoolManager 경유 (프로젝트 풀 관례 통일, 검수 v2 #5). 팝업 실물은 프리팹(TMP+Kostar).
public class DamagePopupSpawner
{
    // 대량 처치 프레임에 Instantiate+TMP 메시 생성 폭풍이 에디터를 질식시킨 실사례 —
    // 시작 시 미리 만들어두면 이후엔 "꺼내서 숫자 교체"만 남는다 (워밍 — 스킵/상한 없이 전부 표시)
    private const int PrewarmCount = 50;

    private readonly ObjectPool<GameObject> pool;

    public DamagePopupSpawner(ObjectPoolManager objectPoolManager, GameObject popupPrefab)
    {
        pool = objectPoolManager.CreateObjectPool(
            popupPrefab,
            createFunc: () => Object.Instantiate(popupPrefab),
            onGet: o => o.SetActive(true),
            onRelease: o => o.SetActive(false));

        var warm = new GameObject[PrewarmCount];
        for (int i = 0; i < PrewarmCount; i++) warm[i] = pool.Get();
        for (int i = 0; i < PrewarmCount; i++) pool.Release(warm[i]);
    }

    public void Show(Vector3 position, int damage, bool isCritical)
    {
        DamagePopup popup = pool.Get().GetComponent<DamagePopup>();
        popup.OnFinished += HandleFinished;
        popup.Show(position, damage, isCritical);
    }

    private void HandleFinished(DamagePopup popup)
    {
        popup.OnFinished -= HandleFinished;
        pool.Release(popup.gameObject);
    }
}
