using UnityEngine;
using UnityEngine.Pool;

public class MonsterSpawner
{
    private readonly ObjectPool<GameObject> pool;

    public MonsterSpawner(ObjectPoolManager objectPoolManager, GameObject prefab)
    {
        pool = objectPoolManager.CreateObjectPool(
            prefab,
            createFunc: () => Object.Instantiate(prefab),
            onGet: obj => obj.SetActive(true),
            onRelease: obj => obj.SetActive(false));
    }

    public Monster Spawn(Vector3 position, int maxHp)
    {
        GameObject obj = pool.Get();
        obj.transform.position = position;
        // 풀 재사용 텔레포트 방지: SetActive 시점에 물리 바디는 "꺼질 때 위치"(예: 플레이어에 박은 자리)로
        // 깨어나고, transform 변경이 물리에 반영되기 전에 MovePosition이 옛 위치 기준으로 먼저 돌면
        // 몬스터가 그 자리로 끌려가 스폰 즉시 도달 판정 → 재돌진하는 실버그. rb.position은 즉시 반영된다
        obj.GetComponent<Rigidbody2D>().position = position;
        Monster monster = obj.GetComponent<Monster>();
        monster.Initialize(maxHp);
        return monster;
    }

    public void Release(Monster monster) => pool.Release(monster.gameObject);
}
