using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Un slot de mejora en el menú de upgrades.
// UpgradesMenuManager lo crea y llama a Refresh() cuando algo cambia.
public class UpgradeSlotUI : MonoBehaviour
{
    public UpgradeType upgradeType;

    [SerializeField] public TextMeshProUGUI nameText;
    [SerializeField] public TextMeshProUGUI starsText;
    [SerializeField] public TextMeshProUGUI bonusText;
    [SerializeField] public TextMeshProUGUI costText;
    [SerializeField] public Button          buyButton;
    [SerializeField] public TextMeshProUGUI buyButtonText;

    // Llamado por UpgradesMenuManager al inicializar y después de comprar
    public void Refresh()
    {
        var cfg   = PersistentData.GetConfig(upgradeType);
        int level = PersistentData.GetLevel(upgradeType);
        float totalBonus = PersistentData.GetTotalBonus(upgradeType);
        int   nextCost   = PersistentData.NextCost(upgradeType);
        bool  canBuy     = PersistentData.CanUpgrade(upgradeType);
        bool  maxed      = level >= PersistentData.MaxUpgradeLevel;

        if (nameText  != null) nameText.text  = cfg.displayName;

        // Estrellas: ★ llenas + ☆ vacías
        if (starsText != null)
        {
            string stars = "";
            for (int i = 0; i < PersistentData.MaxUpgradeLevel; i++)
                stars += i < level ? "★" : "☆";
            starsText.text = stars;
        }

        if (bonusText != null)
        {
            if (totalBonus == 0f)
                bonusText.text = cfg.description;
            else if (cfg.unit == "%")
                bonusText.text = $"+{totalBonus * 100f:F0}% {cfg.description.ToLower()}";
            else
                bonusText.text = $"+{totalBonus:F1}{cfg.unit}";
        }

        if (costText != null)
            costText.text = maxed ? "MÁXIMO" : $"{nextCost} 🪙";

        if (buyButton != null)
        {
            buyButton.interactable = canBuy;
            if (buyButtonText != null)
                buyButtonText.text = maxed ? "—" : "COMPRAR";
        }
    }
}
