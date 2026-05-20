using UnityEngine;
using System.Collections.Generic;
using System;

public class ItemBag : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // DATOS DEL INVENTARIO
    // ─────────────────────────────────────────────

    // Guarda qué items tiene el jugador y en qué nivel está cada uno
    // La key es el ItemData, el value es el nivel actual (1 a 6)
    private Dictionary<ItemData, int> inventory = new Dictionary<ItemData, int>();

    // Lista separada para acceso rápido por tipo
    private List<ItemData> activeItems = new List<ItemData>();
    private List<ItemData> passiveItems = new List<ItemData>();

    // Referencia a los stats del jugador para aplicar efectos pasivos
    private PlayerStats playerStats;

    // ─────────────────────────────────────────────
    // EVENTOS
    // ─────────────────────────────────────────────

    // Se dispara cuando se agrega o mejora un item
    // Otros sistemas (armas) escuchan esto para actualizarse
    public event Action<ItemData, int> OnItemAdded; // item, nivelNuevo

    // ─────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────

    [Header("Items iniciales")]
    [SerializeField] private ItemData startingWeapon;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();

        // Dar el arma inicial al jugador
        if (startingWeapon != null)
            AddItem(startingWeapon);
    }

    // ─────────────────────────────────────────────
    // MÉTODOS PÚBLICOS
    // ─────────────────────────────────────────────

    // Agregar un item nuevo o mejorar uno existente
    public void AddItem(ItemData item)
    {
        if (item == null) return;

        if (HasItem(item))
        {
            // Ya lo tiene: subir de nivel si no está al máximo
            UpgradeItem(item);
        }
        else
        {
            // Es nuevo: agregarlo al inventario en nivel 1
            AddNewItem(item);
        }
    }

    // ¿El jugador tiene este item?
    public bool HasItem(ItemData item)
    {
        return inventory.ContainsKey(item);
    }

    // ¿En qué nivel está este item? Devuelve 0 si no lo tiene
    public int GetItemLevel(ItemData item)
    {
        if (inventory.TryGetValue(item, out int level))
            return level;
        return 0;
    }

    // ¿Puede mejorar este item? (tiene el item y no está al máximo)
    public bool CanUpgrade(ItemData item)
    {
        if (!HasItem(item)) return false;
        return inventory[item] < item.maxLevel;
    }

    // Devuelve todos los items del inventario
    public List<ItemData> GetAllItems()
    {
        return new List<ItemData>(inventory.Keys);
    }

    // Devuelve solo los items activos
    public List<ItemData> GetActiveItems()
    {
        return new List<ItemData>(activeItems);
    }

    // Devuelve solo los items pasivos
    public List<ItemData> GetPassiveItems()
    {
        return new List<ItemData>(passiveItems);
    }

    // Devuelve los items que se pueden mejorar (para el sistema de nivel)
    public List<ItemData> GetUpgradeableItems()
    {
        List<ItemData> upgradeable = new List<ItemData>();
        foreach (var pair in inventory)
        {
            if (pair.Value < pair.Key.maxLevel)
                upgradeable.Add(pair.Key);
        }
        return upgradeable;
    }

    // ─────────────────────────────────────────────
    // MÉTODOS PRIVADOS
    // ─────────────────────────────────────────────

    private void AddNewItem(ItemData item)
    {
        // Agregar al diccionario en nivel 1
        inventory.Add(item, 1);

        // Agregar a la lista correspondiente según tipo
        if (item.itemType == ItemType.Active)
            activeItems.Add(item);
        else
            passiveItems.Add(item);

        // Aplicar el efecto del nivel 1
        ApplyItemEffect(item, 1);

        // Avisar a todos los que escuchan (las armas lo usan)
        OnItemAdded?.Invoke(item, 1);

        Debug.Log($"Item agregado: {item.itemName} nivel 1");
    }

    private void UpgradeItem(ItemData item)
    {
        int currentLevel = inventory[item];

        // Verificar que no esté al máximo
        if (currentLevel >= item.maxLevel)
        {
            Debug.Log($"{item.itemName} ya está al nivel máximo");
            return;
        }

        // Subir de nivel
        int newLevel = currentLevel + 1;
        inventory[item] = newLevel;

        // Aplicar el efecto del nuevo nivel
        ApplyItemEffect(item, newLevel);

        // Avisar
        OnItemAdded?.Invoke(item, newLevel);

        Debug.Log($"Item mejorado: {item.itemName} nivel {newLevel}");
    }

    // Aplica los efectos según el tipo de item
    // Los activos se manejan en sus propios scripts de arma
    // Los pasivos se aplican directamente a PlayerStats
    private void ApplyItemEffect(ItemData item, int level)
    {
        // El índice del array es nivel - 1
        // nivel 1 = levels[0], nivel 2 = levels[1], etc.
        int index = level - 1;

        // Verificar que el array tenga datos para este nivel
        if (item.levels == null || index >= item.levels.Length)
        {
            Debug.LogWarning($"{item.itemName}: No hay datos para el nivel {level}");
            return;
        }

        LevelData levelData = item.levels[index];

        // Solo aplicamos efectos automáticos para pasivos
        // Los activos (armas) tienen sus propios scripts que leen ItemData
        if (item.itemType == ItemType.Passive)
            ApplyPassiveEffect(item, levelData, level);
    }

    // Aplica efectos pasivos a PlayerStats
    // Cada pasiva usa diferentes campos de LevelData
    private void ApplyPassiveEffect(ItemData item, LevelData levelData, int level)
    {
        if (playerStats == null) return;

        switch (item.itemName)
        {
            case "Velocidad de Movimiento":
                playerStats.AddBonusSpeed(levelData.speed);
                break;

            case "Daño General":
                playerStats.AddDamageMultiplier(levelData.damage);
                break;

            case "Velocidad de Disparo":
                playerStats.AddAttackSpeedMultiplier(levelData.speed);
                break;

            // Los orbes los maneja PassiveOrbSystem
            // ItemBag solo dispara el evento OnItemAdded
            // y PassiveOrbSystem lo escucha y actúa
            case "Orbe de la Fortuna":
            case "Orbe de Vida":
                break;

            default:
                break;
        }
    }

}