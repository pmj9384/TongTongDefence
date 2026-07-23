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
        if (result.skin == null) return;   // 코인 부족 — 버튼 비활성이 1차 방어지만, 서비스의 null 계약도 지킨다 (검수 v5 #6)
        gachaSingleResultPopup.ShowWithResult(result.skin, result.isNew);
    }

    private void OnDrawTen()
    {
        var results = GachaService.DrawTen();
        if (results == null) return;   // 위와 동일
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
