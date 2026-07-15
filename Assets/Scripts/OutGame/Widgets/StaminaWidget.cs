using TMPro;

public class StaminaWidget : UIWidget
{
    private TMP_Text staminaText;

    private void Awake() => staminaText = GetComponent<TMP_Text>();

    protected override void Subscribe()
    {
        GameDataManager.Instance.StaminaSystem.onStaminaChanged += UpdateUI;
        Refresh();
    }

    protected override void Unsubscribe()
    {
        GameDataManager.Instance.StaminaSystem.onStaminaChanged -= UpdateUI;
    }

    public override void Refresh()
        => UpdateUI(GameDataManager.Instance.StaminaSystem.CurrentStamina);

    private void UpdateUI(int stamina)
        => staminaText.text = $"{stamina}/{StaminaSystem.MaxStamina}";
}
