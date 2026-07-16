using UnityEngine;
using UnityEngine.UI;

public class ShopScreen : UIScreen
{
    [SerializeField] Button drawOneButton;
    [SerializeField] Button drawTenButton;
    [SerializeField] GachaSingleResultPopup gachaSingleResultPopup;
    [SerializeField] GachaTenResultPopup gachaTenResultPopup;



    private void Awake()
    {
        drawOneButton.onClick.AddListener(OnDrawOne);
        drawTenButton.onClick.AddListener(OnDrawTen);
    }


    private void OnDrawOne()
    {
        var result = GachaService.DrawOne();
        gachaSingleResultPopup.ShowWithResult(result.skin, result.isNew);
    }

    private void OnDrawTen()
    {
        var results = GachaService.DrawTen();
        gachaTenResultPopup.ShowWithResults(results);
    }

    public override void Open()
    {
        base.Open();
        OnCoinsChanged(0);
        GameDataManager.Instance.PlayerAccountData.OnCoinsChanged += OnCoinsChanged;

    }

    public override void Close()
    {
        base.Close();
        GameDataManager.Instance.PlayerAccountData.OnCoinsChanged -= OnCoinsChanged;
    }
    private void OnCoinsChanged(int _)
    {
        drawOneButton.interactable = GachaService.CanDrawOne();
        drawTenButton.interactable = GachaService.CanDrawTen();
    }

}
