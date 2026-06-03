using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeSlotUI : MonoBehaviour
{
    public UpgradeType upgradeType;

    [SerializeField] public TextMeshProUGUI nameText;
    [SerializeField] public TextMeshProUGUI levelText;
    [SerializeField] public TextMeshProUGUI bonusText;
    [SerializeField] public TextMeshProUGUI nextLevelText;
    [SerializeField] public TextMeshProUGUI costText;
    [SerializeField] public Button          buyButton;
    [SerializeField] public TextMeshProUGUI buyButtonText;
    [SerializeField] public Image           slotBackground;
    [SerializeField] public Image           accentBar;
    [SerializeField] public Image[]         levelPips;

    private static readonly Color PipFilled  = new Color(1.00f, 0.80f, 0.10f, 1f);
    private static readonly Color PipEmpty   = new Color(0.15f, 0.15f, 0.22f, 1f);
    private static readonly Color PipMaxed   = new Color(0.25f, 0.90f, 0.45f, 1f);

    private static readonly Color AccentBuy  = new Color(0.15f, 0.55f, 0.95f, 1f);
    private static readonly Color AccentPoor = new Color(0.28f, 0.28f, 0.35f, 1f);
    private static readonly Color AccentMax  = new Color(0.20f, 0.75f, 0.35f, 1f);

    private static readonly Color SlotNormal = new Color(0.10f, 0.16f, 0.28f, 1f);
    private static readonly Color SlotMaxed  = new Color(0.08f, 0.20f, 0.12f, 1f);

    private static readonly Color BtnBuy     = new Color(0.15f, 0.55f, 0.95f, 1f);
    private static readonly Color BtnPoor    = new Color(0.22f, 0.22f, 0.30f, 1f);
    private static readonly Color BtnMaxed   = new Color(0.12f, 0.38f, 0.18f, 1f);

    public void Refresh()
    {
        var   cfg        = PersistentData.GetConfig(upgradeType);
        int   level      = PersistentData.GetLevel(upgradeType);
        float totalBonus = PersistentData.GetTotalBonus(upgradeType);
        int   nextCost   = PersistentData.NextCost(upgradeType);
        bool  canBuy     = PersistentData.CanUpgrade(upgradeType);
        bool  maxed      = level >= PersistentData.MaxUpgradeLevel;

        // Nombre
        if (nameText != null)
            nameText.text = cfg.displayName.ToUpper();

        // Etiqueta de nivel
        if (levelText != null)
        {
            levelText.color = maxed ? AccentMax : new Color(1f, 0.85f, 0.20f);
            levelText.text  = maxed
                ? "MAXIMO"
                : $"Nivel {level} / {PersistentData.MaxUpgradeLevel}";
        }

        // Pips de nivel (cuadros coloreados)
        if (levelPips != null)
        {
            Color filled = maxed ? PipMaxed : PipFilled;
            for (int i = 0; i < levelPips.Length; i++)
            {
                if (levelPips[i] == null) continue;
                levelPips[i].color = i < level ? filled : PipEmpty;
            }
        }

        // Bono actual
        if (bonusText != null)
        {
            if (level == 0)
            {
                bonusText.color = new Color(0.60f, 0.60f, 0.65f);
                bonusText.text  = cfg.description;
            }
            else if (cfg.unit == "%")
            {
                bonusText.color = new Color(0.40f, 0.95f, 0.55f);
                bonusText.text  = $"+{totalBonus * 100f:F0}%  {cfg.description.ToLower()}";
            }
            else
            {
                bonusText.color = new Color(0.40f, 0.95f, 0.55f);
                bonusText.text  = $"+{totalBonus:F1}{cfg.unit}  {cfg.description.ToLower()}";
            }
        }

        // Vista previa del siguiente nivel
        if (nextLevelText != null)
        {
            if (maxed)
            {
                nextLevelText.text = "";
            }
            else if (cfg.unit == "%")
            {
                float next = (level + 1) * cfg.bonusPerLevel * 100f;
                nextLevelText.text = $"Siguiente nivel:  +{cfg.bonusPerLevel * 100f:F0}%  (total {next:F0}%)";
            }
            else
            {
                float next = (level + 1) * cfg.bonusPerLevel;
                nextLevelText.text = $"Siguiente nivel:  +{cfg.bonusPerLevel:F1}{cfg.unit}  (total {next:F1}{cfg.unit})";
            }
        }

        // Costo
        if (costText != null)
            costText.text = maxed ? "" : $"{nextCost}\nmonedas";

        // Boton
        if (buyButton != null)
        {
            buyButton.interactable = canBuy;
            var colors = buyButton.colors;
            colors.normalColor   = maxed ? BtnMaxed : (canBuy ? BtnBuy : BtnPoor);
            colors.disabledColor = maxed ? BtnMaxed : BtnPoor;
            buyButton.colors = colors;

            if (buyButtonText != null)
                buyButtonText.text = maxed ? "MAX" : "SUBIR";
        }

        // Franja de acento izquierda
        if (accentBar != null)
            accentBar.color = maxed ? AccentMax : (canBuy ? AccentBuy : AccentPoor);

        // Fondo del slot
        if (slotBackground != null)
            slotBackground.color = maxed ? SlotMaxed : SlotNormal;
    }
}
