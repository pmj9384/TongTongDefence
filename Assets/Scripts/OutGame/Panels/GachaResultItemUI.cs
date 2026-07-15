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
        skinIcon.sprite = Resources.Load<Sprite>($"Sprites/Skins/{data.SkinId}");
        newBadge.SetActive(isNew);
    }
}
