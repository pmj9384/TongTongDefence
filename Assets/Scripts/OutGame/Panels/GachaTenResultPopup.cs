using System.Collections.Generic;
using UnityEngine;

public class GachaTenResultPopup : GachaResultPopupBase
{
    [SerializeField] private List<GachaResultItemUI> slots;

    public void ShowWithResults(List<(SkinDataTable.SkinRawData skin, bool isNew)> results)
    {
        for (int i = 0; i < results.Count; i++)
            slots[i].Setup(results[i].skin, results[i].isNew);
        Show();
    }
}
