using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UpgradesMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject       upgradesPanel;
    [SerializeField] private Transform        slotsContainer;
    [SerializeField] private TextMeshProUGUI  coinsText;

    private readonly List<UpgradeSlotUI> slots = new List<UpgradeSlotUI>();

    // ── Colores ────────────────────────────────────────────────────────
    private static readonly Color BgColor      = new Color(0.08f, 0.12f, 0.22f, 0.97f);
    private static readonly Color SlotColor    = new Color(0.12f, 0.18f, 0.32f, 1f);
    private static readonly Color BtnColor     = new Color(0.15f, 0.55f, 0.95f, 1f);
    private static readonly Color BtnDisabled  = new Color(0.25f, 0.25f, 0.30f, 1f);

    void Start()
    {
        if (upgradesPanel != null)
            upgradesPanel.SetActive(false);

        BuildSlots();
        RefreshAll();
    }

    // ── Construir slots una vez ────────────────────────────────────────
    private void BuildSlots()
    {
        if (slotsContainer == null) return;

        foreach (var cfg in PersistentData.Upgrades)
        {
            var slot = CreateSlot(cfg);
            slots.Add(slot);
        }
    }

    private UpgradeSlotUI CreateSlot(UpgradeConfig cfg)
    {
        // Contenedor del slot
        var go     = new GameObject(cfg.displayName + "_Slot");
        go.transform.SetParent(slotsContainer, false);
        var bg     = go.AddComponent<Image>();
        bg.color   = SlotColor;
        var rt     = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0f, 68f);

        var slot       = go.AddComponent<UpgradeSlotUI>();
        slot.upgradeType = cfg.type;

        // Nombre
        slot.nameText  = AddText(go, "Name",  new Vector2(0f, 0.5f), new Vector2(0.25f, 0.5f),
                                  new Vector2(8f, -4f), new Vector2(-4f, 4f), 16f, TMPro.FontStyles.Bold,
                                  TMPro.TextAlignmentOptions.MidlineLeft, Color.white);

        // Estrellas
        slot.starsText = AddText(go, "Stars", new Vector2(0.25f, 0.5f), new Vector2(0.45f, 0.5f),
                                  new Vector2(4f, -4f), new Vector2(-4f, 4f), 18f, TMPro.FontStyles.Normal,
                                  TMPro.TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.1f));

        // Bono actual
        slot.bonusText = AddText(go, "Bonus", new Vector2(0.45f, 0.5f), new Vector2(0.72f, 0.5f),
                                  new Vector2(4f, -4f), new Vector2(-4f, 4f), 12f, TMPro.FontStyles.Normal,
                                  TMPro.TextAlignmentOptions.MidlineLeft, new Color(0.7f, 0.9f, 0.7f));

        // Coste
        slot.costText  = AddText(go, "Cost",  new Vector2(0.72f, 0.5f), new Vector2(0.85f, 0.5f),
                                  new Vector2(4f, -4f), new Vector2(-4f, 4f), 14f, TMPro.FontStyles.Bold,
                                  TMPro.TextAlignmentOptions.Center, new Color(1f, 0.9f, 0.3f));

        // Botón comprar
        var btnGO   = new GameObject("BuyButton");
        btnGO.transform.SetParent(go.transform, false);
        var btnImg  = btnGO.AddComponent<Image>();
        btnImg.color = BtnColor;
        var btn     = btnGO.AddComponent<Button>();
        var btnRT   = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.85f, 0.15f);
        btnRT.anchorMax = new Vector2(1f,    0.85f);
        btnRT.offsetMin = new Vector2(4f, 0f);
        btnRT.offsetMax = new Vector2(-8f, 0f);

        var btnLblGO = new GameObject("Label");
        btnLblGO.transform.SetParent(btnGO.transform, false);
        var btnLbl   = btnLblGO.AddComponent<TMPro.TextMeshProUGUI>();
        btnLbl.text      = "COMPRAR";
        btnLbl.fontSize  = 12f;
        btnLbl.fontStyle = TMPro.FontStyles.Bold;
        btnLbl.alignment = TMPro.TextAlignmentOptions.Center;
        btnLbl.color     = Color.white;
        var lblRT = btnLblGO.GetComponent<RectTransform>();
        lblRT.anchorMin = Vector2.zero;
        lblRT.anchorMax = Vector2.one;
        lblRT.offsetMin = Vector2.zero;
        lblRT.offsetMax = Vector2.zero;

        slot.buyButton     = btn;
        slot.buyButtonText = btnLbl;

        // Conectar botón
        var capturedType = cfg.type;
        btn.onClick.AddListener(() => OnBuy(capturedType));

        // Colores del botón (disabled)
        var colors      = btn.colors;
        colors.disabledColor = BtnDisabled;
        btn.colors      = colors;

        return slot;
    }

    // Helper para crear texto dentro de un slot
    private TMPro.TextMeshProUGUI AddText(GameObject parent, string name,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax,
        float size, TMPro.FontStyles style,
        TMPro.TextAlignmentOptions align, Color color)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var tmp = go.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.fontSize  = size;
        tmp.fontStyle = style;
        tmp.alignment = align;
        tmp.color     = color;
        var rt  = go.GetComponent<RectTransform>();
        rt.anchorMin  = new Vector2(anchorMin.x, 0f);
        rt.anchorMax  = new Vector2(anchorMax.x, 1f);
        rt.offsetMin  = offsetMin;
        rt.offsetMax  = offsetMax;
        return tmp;
    }

    // ── Lógica ────────────────────────────────────────────────────────
    private void OnBuy(UpgradeType type)
    {
        if (PersistentData.TryPurchase(type))
            RefreshAll();
    }

    private void RefreshAll()
    {
        if (coinsText != null)
            coinsText.text = $"🪙  {PersistentData.Coins} monedas";

        foreach (var slot in slots)
            slot.Refresh();
    }

    public void OpenPanel()
    {
        if (upgradesPanel != null) upgradesPanel.SetActive(true);
        RefreshAll();
    }

    public void ClosePanel()
    {
        if (upgradesPanel != null) upgradesPanel.SetActive(false);
    }
}
