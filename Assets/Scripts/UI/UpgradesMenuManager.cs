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

    private static readonly Color DividerColor = new Color(1f, 1f, 1f, 0.08f);

    void Start()
    {
        if (upgradesPanel != null)
            upgradesPanel.SetActive(false);

        BuildSlots();
        RefreshAll();
    }

    private void BuildSlots()
    {
        if (slotsContainer == null) return;
        foreach (var cfg in PersistentData.Upgrades)
            slots.Add(CreateSlot(cfg));
    }

    private UpgradeSlotUI CreateSlot(UpgradeConfig cfg)
    {
        int maxLevel = PersistentData.MaxUpgradeLevel;

        // ── Fondo del slot ─────────────────────────────────────────────
        var go = new GameObject(cfg.displayName + "_Slot");
        go.transform.SetParent(slotsContainer, false);
        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.10f, 0.16f, 0.28f, 1f);
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 155f);

        var slot            = go.AddComponent<UpgradeSlotUI>();
        slot.upgradeType    = cfg.type;
        slot.slotBackground = bg;

        // ── Franja de acento izquierda ─────────────────────────────────
        var accentGO  = new GameObject("Accent");
        accentGO.transform.SetParent(go.transform, false);
        var accentImg = accentGO.AddComponent<Image>();
        accentImg.color = new Color(0.15f, 0.55f, 0.95f, 1f);
        var accentRT  = accentGO.GetComponent<RectTransform>();
        accentRT.anchorMin = new Vector2(0f, 0f);
        accentRT.anchorMax = new Vector2(0f, 1f);
        accentRT.offsetMin = new Vector2(0f, 0f);
        accentRT.offsetMax = new Vector2(6f, 0f);
        slot.accentBar = accentImg;

        // ── Fila superior: Nombre + Nivel ──────────────────────────────
        slot.nameText = AddLabel(go, "Name",
            new Vector2(0.03f, 0.62f), new Vector2(0.60f, 1f),
            new Vector2(10f, 0f), new Vector2(-4f, -6f),
            19f, FontStyles.Bold, TextAlignmentOptions.BottomLeft, Color.white);

        slot.levelText = AddLabel(go, "Level",
            new Vector2(0.60f, 0.62f), new Vector2(0.99f, 1f),
            new Vector2(0f, 0f), new Vector2(-10f, -6f),
            15f, FontStyles.Bold, TextAlignmentOptions.BottomRight,
            new Color(1f, 0.85f, 0.20f));

        // ── Fila de pips (indicadores de nivel) ────────────────────────
        var pipsCont = new GameObject("PipsContainer", typeof(RectTransform));
        pipsCont.transform.SetParent(go.transform, false);
        var pipsContRT = pipsCont.GetComponent<RectTransform>();
        pipsContRT.anchorMin = new Vector2(0.03f, 0.48f);
        pipsContRT.anchorMax = new Vector2(0.99f, 0.62f);
        pipsContRT.offsetMin = new Vector2(10f,  2f);
        pipsContRT.offsetMax = new Vector2(-10f, -2f);

        var hlg = pipsCont.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing              = 5f;
        hlg.childAlignment       = TextAnchor.MiddleLeft;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth    = false;
        hlg.childControlHeight   = true;
        hlg.padding              = new RectOffset(0, 0, 0, 0);

        slot.levelPips = new Image[maxLevel];
        for (int i = 0; i < maxLevel; i++)
        {
            var pipGO  = new GameObject($"Pip{i}");
            pipGO.transform.SetParent(pipsCont.transform, false);
            var pipImg = pipGO.AddComponent<Image>();
            pipImg.color = new Color(0.15f, 0.15f, 0.22f, 1f);
            var pipLE  = pipGO.AddComponent<LayoutElement>();
            pipLE.preferredWidth  = 32f;
            pipLE.flexibleWidth   = 1f;
            slot.levelPips[i] = pipImg;
        }

        // ── Divisor ────────────────────────────────────────────────────
        var divGO  = new GameObject("Divider");
        divGO.transform.SetParent(go.transform, false);
        var divImg = divGO.AddComponent<Image>();
        divImg.color = DividerColor;
        var divRT  = divGO.GetComponent<RectTransform>();
        divRT.anchorMin = new Vector2(0.03f, 0.455f);
        divRT.anchorMax = new Vector2(0.97f, 0.470f);
        divRT.offsetMin = new Vector2(10f, 0f);
        divRT.offsetMax = new Vector2(-10f, 0f);

        // ── Fila inferior: Bonus + Siguiente | Costo | Boton ──────────
        // Bono actual (izquierda, fila alta)
        slot.bonusText = AddLabel(go, "Bonus",
            new Vector2(0.03f, 0.24f), new Vector2(0.58f, 0.44f),
            new Vector2(10f, 0f), new Vector2(-4f, 0f),
            13f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft,
            new Color(0.40f, 0.95f, 0.55f));

        // Vista previa del siguiente nivel (izquierda, fila baja)
        slot.nextLevelText = AddLabel(go, "NextLevel",
            new Vector2(0.03f, 0.04f), new Vector2(0.58f, 0.23f),
            new Vector2(10f, 0f), new Vector2(-4f, 0f),
            11f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft,
            new Color(0.55f, 0.65f, 0.75f));

        // Costo (centro-derecha, ambas filas)
        slot.costText = AddLabel(go, "Cost",
            new Vector2(0.58f, 0.04f), new Vector2(0.72f, 0.44f),
            new Vector2(4f, 0f), new Vector2(-4f, 0f),
            13f, FontStyles.Bold, TextAlignmentOptions.Center,
            new Color(1f, 0.88f, 0.25f));

        // ── Boton SUBIR ────────────────────────────────────────────────
        var btnGO  = new GameObject("BuyButton");
        btnGO.transform.SetParent(go.transform, false);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.15f, 0.55f, 0.95f, 1f);
        var btn    = btnGO.AddComponent<Button>();
        var btnRT  = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.72f, 0.06f);
        btnRT.anchorMax = new Vector2(0.99f, 0.44f);
        btnRT.offsetMin = new Vector2(6f,   0f);
        btnRT.offsetMax = new Vector2(-10f, 0f);

        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(btnGO.transform, false);
        var lbl   = lblGO.AddComponent<TextMeshProUGUI>();
        lbl.text      = "SUBIR";
        lbl.fontSize  = 16f;
        lbl.fontStyle = FontStyles.Bold;
        lbl.alignment = TextAlignmentOptions.Center;
        lbl.color     = Color.white;
        var lblRT = lblGO.GetComponent<RectTransform>();
        lblRT.anchorMin = Vector2.zero;
        lblRT.anchorMax = Vector2.one;
        lblRT.offsetMin = Vector2.zero;
        lblRT.offsetMax = Vector2.zero;

        slot.buyButton     = btn;
        slot.buyButtonText = lbl;

        var colors = btn.colors;
        colors.disabledColor    = new Color(0.22f, 0.22f, 0.30f, 1f);
        colors.highlightedColor = new Color(0.30f, 0.70f, 1.00f, 1f);
        btn.colors = colors;

        var capturedType = cfg.type;
        btn.onClick.AddListener(() => OnBuy(capturedType));

        return slot;
    }

    private TextMeshProUGUI AddLabel(GameObject parent, string name,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax,
        float size, FontStyles style,
        TextAlignmentOptions align, Color color)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize  = size;
        tmp.fontStyle = style;
        tmp.alignment = align;
        tmp.color     = color;
        var rt  = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        return tmp;
    }

    private void OnBuy(UpgradeType type)
    {
        if (PersistentData.TryPurchase(type))
            RefreshAll();
    }

    private void RefreshAll()
    {
        if (coinsText != null)
            coinsText.text = $"Monedas:  {PersistentData.Coins}";

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
