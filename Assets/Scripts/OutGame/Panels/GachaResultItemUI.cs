using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaResultItemUI : MonoBehaviour
{
    [SerializeField] private Image skinIcon;
    [SerializeField] private TMP_Text skinNameText;
    [SerializeField] private GameObject newBadge;

    public void Setup(SkinDataTable.SkinRawData data, bool isNew)
    {

        skinNameText.text = data.SkinName;
        skinIcon.sprite = SkinSprites.Load($"Skins/{data.SkinId}");   // Addressables 전환 (이식 개조 2026-07-15)
        newBadge.SetActive(isNew);
    }
}
