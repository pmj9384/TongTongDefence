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
        if (!GameDataManager.HasInstance) return;   // 부팅 Setup/teardown — Instance 접근이 초기화 전 생성을 유발하므로 금지
        GameDataManager.Instance.PlayerAccountData.OnCoinsChanged -= OnCoinsChanged;
    }
    private void OnCoinsChanged(int _)
    {
        drawOneButton.interactable = GachaService.CanDrawOne();
        drawTenButton.interactable = GachaService.CanDrawTen();
    }

}
