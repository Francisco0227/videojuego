using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class WeaponBarUI : MonoBehaviour
{
    [SerializeField] private Transform slotsContainer;

    private ItemBag itemBag;
    private readonly Dictionary<ItemData, TextMeshProUGUI> levelLabels = new Dictionary<ItemData, TextMeshProUGUI>();
    private readonly Dictionary<ItemData, Image>           slotBgs     = new Dictionary<ItemData, Image>();

    // Colores según tipo de ítem
    private static readonly Color ColorActive  = new Color(0.10f, 0.30f, 0.65f, 0.92f);
    private static readonly Color ColorPassive = new Color(0.08f, 0.42f, 0.22f, 0.92f);

    void Start()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            itemBag = player.GetComponent<ItemBag>();
            if (itemBag != null)
                itemBag.OnItemAdded += OnItemChanged;
        }
    }

    void OnDestroy()
    {
        if (itemBag != null)
            itemBag.OnItemAdded -= OnItemChanged;
    }

    private void OnItemChanged(ItemData item, int level)
    {
        if (levelLabels.ContainsKey(item))
            UpdateSlot(item, level);
        else
            CreateSlot(item, level);
    }

    private void CreateSlot(ItemData item, int level)
    {
        var target = slotsContainer != null ? slotsContainer : transform;

        // Contenedor del slot
        var slotGO  = new GameObject(item.itemName);
        slotGO.transform.SetParent(target, false);

        var bg      = slotGO.AddComponent<Image>();
        bg.color    = item.itemType == ItemType.Active ? ColorActive : ColorPassive;
        var rt      = slotGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(88f, 54f);

        // Nombre del arma (arriba)
        var nameGO  = new GameObject("Name");
        nameGO.transform.SetParent(slotGO.transform, false);
        var nameTxt = nameGO.AddComponent<TextMeshProUGUI>();
        nameTxt.text      = Shorten(item.itemName);
        nameTxt.fontSize  = 10f;
        nameTxt.alignment = TextAlignmentOptions.Center;
        nameTxt.color     = Color.white;
        var nameRect      = nameGO.GetComponent<RectTransform>();
        nameRect.anchorMin        = new Vector2(0f, 0.38f);
        nameRect.anchorMax        = new Vector2(1f, 1f);
        nameRect.offsetMin        = new Vector2(3f,  0f);
        nameRect.offsetMax        = new Vector2(-3f, -3f);

        // Badge de nivel (abajo)
        var badgeGO  = new GameObject("Badge");
        badgeGO.transform.SetParent(slotGO.transform, false);
        var badgeBg  = badgeGO.AddComponent<Image>();
        badgeBg.color = new Color(1f, 0.78f, 0f);
        var badgeRect = badgeGO.GetComponent<RectTransform>();
        badgeRect.anchorMin        = new Vector2(0.5f, 0f);
        badgeRect.anchorMax        = new Vector2(0.5f, 0f);
        badgeRect.pivot            = new Vector2(0.5f, 0f);
        badgeRect.sizeDelta        = new Vector2(48f, 17f);
        badgeRect.anchoredPosition = new Vector2(0f, 4f);

        var lvGO   = new GameObject("LvText");
        lvGO.transform.SetParent(badgeGO.transform, false);
        var lvTxt  = lvGO.AddComponent<TextMeshProUGUI>();
        lvTxt.text      = "Lv " + level;
        lvTxt.fontSize  = 11f;
        lvTxt.fontStyle = FontStyles.Bold;
        lvTxt.alignment = TextAlignmentOptions.Center;
        lvTxt.color     = Color.black;
        var lvRect      = lvGO.GetComponent<RectTransform>();
        lvRect.anchorMin = Vector2.zero;
        lvRect.anchorMax = Vector2.one;
        lvRect.offsetMin = Vector2.zero;
        lvRect.offsetMax = Vector2.zero;

        levelLabels[item] = lvTxt;
        slotBgs[item]     = bg;
    }

    private void UpdateSlot(ItemData item, int level)
    {
        if (levelLabels.TryGetValue(item, out var lv))
            lv.text = "Lv " + level;

        if (slotBgs.TryGetValue(item, out var bg))
            StartCoroutine(FlashSlot(bg, item.itemType == ItemType.Active));
    }

    private IEnumerator FlashSlot(Image bg, bool isActive)
    {
        bg.color = Color.white;
        yield return new WaitForSecondsRealtime(0.25f);
        bg.color = isActive ? ColorActive : ColorPassive;
    }

    private static string Shorten(string name) =>
        name.Length > 13 ? name.Substring(0, 12) + "…" : name;
}
