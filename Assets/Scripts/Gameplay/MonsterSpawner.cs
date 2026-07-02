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
        Monster monster = obj.GetComponent<Monster>();
        monster.Initialize(maxHp);
        return monster;
    }

    public void Release(Monster monster) => pool.Release(monster.gameObject);
}
