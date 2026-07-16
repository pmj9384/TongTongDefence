using System.Collections.Generic;
using UnityEngine;

public class SkinScreen : UIScreen
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private SkinItemUI skinItemPrefab;

    private readonly List<SkinItemUI> items = new();

    public override void Open()
    {
        base.Open();
        BuildList();
        GameDataManager.Instance.SkinUserData.OnSkinEquipped += OnSkinEquipped;
    }

    public override void Close()
    {
        base.Close();
        if (!GameDataManager.HasInstance) return;   // 부팅 Setup/teardown — Instance 접근이 초기화 전 생성을 유발하므로 금지
        GameDataManager.Instance.SkinUserData.OnSkinEquipped -= OnSkinEquipped;
    }

    private void BuildList()
    {
        foreach (var item in items)
            Destroy(item.gameObject);
        items.Clear();

        foreach (var data in DataTableManager.SkinDataTable.All)
        {
            var item = Instantiate(skinItemPrefab, contentParent);
            item.Setup(data);
            items.Add(item);
        }
    }

    private void OnSkinEquipped(string _)
    {
        foreach (var item in items)
            item.Refresh();
    }
}
