using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ItemButton : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // REFERENCIAS UI
    // ─────────────────────────────────────────────

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button button;

    // ─────────────────────────────────────────────
    // DATOS
    // ─────────────────────────────────────────────

    private ItemData itemData;
    private Action<ItemData> onSelected;

    // ─────────────────────────────────────────────
    // SETUP
    // Lo llama LevelUpSystem al crear el botón
    // ─────────────────────────────────────────────

    public void Setup(ItemData item, Action<ItemData> callback, ItemBag bag)
    {
        itemData = item;
        onSelected = callback;

        // Nivel actual del item (0 si no lo tiene)
        int currentLevel = bag != null ? bag.GetItemLevel(item) : 0;
        int nextLevel = currentLevel + 1;

        // Configurar textos
        if (nameText != null)
            nameText.text = item.itemName;

        if (levelText != null)
        {
            if (currentLevel == 0)
                levelText.text = "NUEVO";
            else if (currentLevel >= item.maxLevel)
                levelText.text = "MÁX";
            else
                levelText.text = $"Nv {currentLevel} → {nextLevel}";
        }

        // Mostrar la descripción del siguiente nivel
        if (descriptionText != null)
        {
            if (currentLevel < item.maxLevel && item.levels != null
                && nextLevel - 1 < item.levels.Length)
            {
                descriptionText.text = item.levels[nextLevel - 1].levelDescription;
            }
            else
            {
                descriptionText.text = item.description;
            }
        }

        // Conectar el botón
        if (button != null)
            button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        onSelected?.Invoke(itemData);
    }
}