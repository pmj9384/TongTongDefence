using UnityEngine;
using UnityEngine.Pool;

// 데미지 팝업의 풀+배치만 담당 (순수 C#) — MonsterSpawner/FragmentBall 풀과 동일하게
// ObjectPoolManager 경유 (프로젝트 풀 관례 통일, 검수 v2 #5). 팝업 실물은 프리팹(TMP+Kostar).
public class DamagePopupSpawner
{
    private readonly ObjectPool<GameObject> pool;

    public DamagePopupSpawner(ObjectPoolManager objectPoolManager, GameObject popupPrefab)
    {
        pool = objectPoolManager.CreateObjectPool(
            popupPrefab,
            createFunc: () => Object.Instantiate(popupPrefab),
            onGet: o => o.SetActive(true),
            onRelease: o => o.SetActive(false));
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
