using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LevelUpSystem : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // REFERENCIAS
    // ─────────────────────────────────────────────

    [Header("Todos los items del juego")]
    [SerializeField] private List<ItemData> allItems;

    [Header("UI")]
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private GameObject itemButtonPrefab;

    // ─────────────────────────────────────────────
    // REFERENCIAS INTERNAS
    // ─────────────────────────────────────────────

    private ItemBag itemBag;
    private PassiveOrbSystem orbSystem;

    // ─────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            itemBag = player.GetComponent<ItemBag>();
            orbSystem = player.GetComponent<PassiveOrbSystem>();
        }

        // Suscribirse al evento de subida de nivel
        if (ExperienceManager.Instance != null)
            ExperienceManager.Instance.OnLevelUp += OnPlayerLevelUp;

        // El panel empieza oculto
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);
    }

    void OnDestroy()
    {
        if (ExperienceManager.Instance != null)
            ExperienceManager.Instance.OnLevelUp -= OnPlayerLevelUp;
    }

    // ─────────────────────────────────────────────
    // SUBIDA DE NIVEL
    // ─────────────────────────────────────────────

    private void OnPlayerLevelUp(int newLevel)
    {
        // Notificar al sistema de orbes
        if (orbSystem != null)
            orbSystem.OnPlayerLevelUp();

        // Generar las opciones
        List<ItemData> options = GenerateOptions();

        // Mostrar la pantalla de selección
        ShowLevelUpPanel(options);
    }

    // ─────────────────────────────────────────────
    // GENERAR OPCIONES
    // Reglas:
    // 1. Al menos un item del inventario que se pueda mejorar
    // 2. Los otros son aleatorios sin repetir
    // 3. Si el Orbe de Fortuna está en nivel 3+ aparece una opción extra
    // ─────────────────────────────────────────────

    private List<ItemData> GenerateOptions()
    {
        List<ItemData> options = new List<ItemData>();
        int optionCount = 3;

        // El Orbe de Fortuna nivel 3+ agrega una opción extra
        if (orbSystem != null && orbSystem.HasExtraLevelUpOption())
            optionCount = 4;

        // PASO 1: Agregar un item del inventario que se pueda mejorar
        ItemData inventoryItem = GetRandomUpgradeableItem();
        if (inventoryItem != null)
            options.Add(inventoryItem);

        // PASO 2: Rellenar el resto con items aleatorios sin repetir
        int attempts = 0; // Límite para evitar loop infinito
        while (options.Count < optionCount && attempts < 50)
        {
            attempts++;

            ItemData randomItem = GetRandomItem(options);
            if (randomItem != null)
                options.Add(randomItem);
        }

        // Si no hay suficientes items distintos rellenar con
        // items del inventario aunque ya estén al máximo
        while (options.Count < optionCount)
        {
            ItemData fallback = GetFallbackItem(options);
            if (fallback == null) break;
            options.Add(fallback);
        }

        // Mezclar el orden para que el item del inventario
        // no siempre aparezca en la misma posición
        return ShuffleList(options);
    }

    // Un item del inventario que se pueda mejorar
    // El Orbe de Fortuna aumenta la probabilidad de que
    // aparezcan más items del inventario
    private ItemData GetRandomUpgradeableItem()
    {
        if (itemBag == null) return null;

        List<ItemData> upgradeable = itemBag.GetUpgradeableItems();
        if (upgradeable.Count == 0) return null;

        // Elegir uno al azar de los que se pueden mejorar
        int index = Random.Range(0, upgradeable.Count);
        return upgradeable[index];
    }

    // Un item aleatorio de todos los disponibles
    // que no esté ya en las opciones actuales
    private ItemData GetRandomItem(List<ItemData> currentOptions)
    {
        if (allItems == null || allItems.Count == 0) return null;

        // Items que no están ya en las opciones
        List<ItemData> available = allItems
            .Where(item => !currentOptions.Contains(item))
            .ToList();

        if (available.Count == 0) return null;

        // Con el Orbe de Fortuna hay más probabilidad de
        // que el item elegido sea uno del inventario
        float inventoryChance = orbSystem != null
            ? orbSystem.GetInventoryItemChance()
            : 0.33f;

        float roll = Random.Range(0f, 1f);

        if (roll <= inventoryChance)
        {
            // Intentar dar un item del inventario que no esté ya en opciones
            List<ItemData> inventoryItems = itemBag != null
                ? itemBag.GetAllItems()
                    .Where(item => !currentOptions.Contains(item))
                    .ToList()
                : new List<ItemData>();

            if (inventoryItems.Count > 0)
                return inventoryItems[Random.Range(0, inventoryItems.Count)];
        }

        // Item completamente aleatorio
        return available[Random.Range(0, available.Count)];
    }

    // Fallback: item del inventario aunque esté al máximo
    private ItemData GetFallbackItem(List<ItemData> currentOptions)
    {
        if (itemBag == null) return null;

        List<ItemData> allOwned = itemBag.GetAllItems()
            .Where(item => !currentOptions.Contains(item))
            .ToList();

        if (allOwned.Count == 0) return null;

        return allOwned[Random.Range(0, allOwned.Count)];
    }

    // Mezclar una lista aleatoriamente
    // Algoritmo Fisher-Yates
    private List<ItemData> ShuffleList(List<ItemData> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            ItemData tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
        return list;
    }

    // ─────────────────────────────────────────────
    // MOSTRAR Y OCULTAR EL PANEL
    // ─────────────────────────────────────────────

    private void ShowLevelUpPanel(List<ItemData> options)
    {
        if (levelUpPanel == null) return;

        // Pausar el juego
        Time.timeScale = 0f;

        // Mostrar el panel
        levelUpPanel.SetActive(true);

        // Limpiar opciones anteriores
        foreach (Transform child in optionsContainer)
            Destroy(child.gameObject);

        // Crear un botón por cada opción
        foreach (ItemData item in options)
            CreateItemButton(item);
    }

    private void HideLevelUpPanel()
    {
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);

        // Reanudar el juego
        Time.timeScale = 1f;
    }

    // ─────────────────────────────────────────────
    // CREAR BOTONES DE ITEMS
    // ─────────────────────────────────────────────

    private void CreateItemButton(ItemData item)
    {
        if (itemButtonPrefab == null || optionsContainer == null) return;

        GameObject buttonObj = Instantiate(itemButtonPrefab, optionsContainer);

        // Configurar el botón con los datos del item
        ItemButton itemButton = buttonObj.GetComponent<ItemButton>();
        if (itemButton != null)
            itemButton.Setup(item, OnItemSelected, itemBag);
    }

    // ─────────────────────────────────────────────
    // SELECCIÓN DE ITEM
    // ─────────────────────────────────────────────

    private void OnItemSelected(ItemData selectedItem)
    {
        // Agregar el item al inventario del jugador
        if (itemBag != null)
            itemBag.AddItem(selectedItem);

        // Ocultar el panel y reanudar
        HideLevelUpPanel();

        Debug.Log($"Item seleccionado: {selectedItem.itemName}");
    }
}