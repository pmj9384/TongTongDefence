using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkinItemUI : MonoBehaviour
{
    [SerializeField] private Image skinImage;
    [SerializeField] private TMP_Text skinNameText;
    [SerializeField] private TMP_Text gradeText;
    [SerializeField] private Button equipButton;
    [SerializeField] private TMP_Text equipButtonText;
    [SerializeField] private GameObject equippedIndicator;

    private string skinId;

    public void Setup(SkinDataTable.SkinRawData data)
    {
        skinId = data.SkinId;
        skinNameText.text = data.SkinName;
        gradeText.text = data.Grade;
        skinImage.sprite = SkinSprites.Load($"Skins/{data.SkinId}");   // Addressables 전환 (이식 개조 2026-07-15)

        equipButton.onClick.AddListener(OnEquipClicked);
        Refresh();
    }

    public void Refresh()
    {
        bool isEquipped = GameDataManager.Instance.SkinUserData.EquippedSkinId == skinId;
        equippedIndicator.SetActive(isEquipped);
        if (GameDataManager.Instance.SkinUserData.IsOwned(skinId) == false)
        {
            equipButton.interactable = false;
            equipButtonText.text = "잠금";
        }
        else
        {
            equipButton.interactable = !isEquipped;
            equipButtonText.text = isEquipped ? "장착중" : "장착";
        }
    }

    private void OnEquipClicked()
    {
        GameDataManager.Instance.SkinUserData.Equip(skinId);
    }

    private void OnDestroy()
    {
        if (equipButton != null)
            equipButton.onClick.RemoveAllListeners();
    }
}
