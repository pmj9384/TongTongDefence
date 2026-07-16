using TMPro;

public class CoinWidget : UIWidget
{
    private TMP_Text coinText;

    private void Awake() => coinText = GetComponent<TMP_Text>();

    protected override void Subscribe()
    {
        GameDataManager.Instance.PlayerAccountData.OnCoinsChanged += UpdateUI;
        Refresh();
    }

    protected override void Unsubscribe()
    {
        GameDataManager.Instance.PlayerAccountData.OnCoinsChanged -= UpdateUI;
    }

    public override void Refresh()
        => UpdateUI(GameDataManager.Instance.PlayerAccountData.Coins);

    private void UpdateUI(int coins) => coinText.text = coins.ToString("N0");
}
