using UnityEngine;

public enum UpgradeType { Defense, MaxHealth, Speed, PickupRange, Damage }

// Datos de configuración de una mejora permanente
public class UpgradeConfig
{
    public readonly string      displayName;
    public readonly UpgradeType type;
    public readonly int[]       costs;        // coste para pasar del nivel i al i+1
    public readonly float       bonusPerLevel;
    public readonly string      unit;         // "%" o "" para mostrarlo en UI
    public readonly string      description;

    public UpgradeConfig(string name, UpgradeType t, int[] costs,
                         float bonus, string unit, string desc)
    {
        displayName  = name;  this.type = t;  this.costs = costs;
        bonusPerLevel = bonus; this.unit = unit; description = desc;
    }
}

// Clase estática — sin MonoBehaviour, usa PlayerPrefs directamente
public static class PersistentData
{
    public const int MaxUpgradeLevel = 5;

    public static readonly UpgradeConfig[] Upgrades = new UpgradeConfig[]
    {
        new UpgradeConfig("Defensa",         UpgradeType.Defense,
            new[]{50, 75, 100, 150, 200}, 0.05f, "%",
            "Reduce el daño recibido"),
        new UpgradeConfig("Vida Máxima",     UpgradeType.MaxHealth,
            new[]{40, 60,  80, 120, 160}, 25f,  " HP",
            "Aumenta la vida máxima"),
        new UpgradeConfig("Velocidad",       UpgradeType.Speed,
            new[]{35, 52,  70, 105, 140}, 0.5f, "",
            "Aumenta la velocidad de movimiento"),
        new UpgradeConfig("Rango Recogida",  UpgradeType.PickupRange,
            new[]{30, 45,  60,  90, 120}, 0.5f, "",
            "Aumenta el radio para recoger estrellas"),
        new UpgradeConfig("Daño Base",       UpgradeType.Damage,
            new[]{50, 75, 100, 150, 200}, 0.05f, "%",
            "Aumenta el daño de todas las armas"),
    };

    // ── Monedas ────────────────────────────────────────────────────────
    public static int Coins
    {
        get => PlayerPrefs.GetInt("coins", 0);
        set { PlayerPrefs.SetInt("coins", Mathf.Max(0, value)); PlayerPrefs.Save(); }
    }

    public static void AddCoins(int amount)  { Coins += amount; }
    public static bool SpendCoins(int amount)
    {
        if (Coins < amount) return false;
        Coins -= amount;
        return true;
    }

    // ── Niveles de mejora ──────────────────────────────────────────────
    public static int GetLevel(UpgradeType t)
        => PlayerPrefs.GetInt("upg_" + t, 0);

    private static void SetLevel(UpgradeType t, int level)
    { PlayerPrefs.SetInt("upg_" + t, level); PlayerPrefs.Save(); }

    // Bono total acumulado hasta el nivel actual
    public static float GetTotalBonus(UpgradeType t)
        => GetLevel(t) * GetConfig(t).bonusPerLevel;

    // Coste del siguiente nivel (0 si ya está al máximo)
    public static int NextCost(UpgradeType t)
    {
        int lvl = GetLevel(t);
        return lvl < MaxUpgradeLevel ? GetConfig(t).costs[lvl] : 0;
    }

    public static bool CanUpgrade(UpgradeType t)
        => GetLevel(t) < MaxUpgradeLevel && Coins >= NextCost(t);

    // Intenta comprar la siguiente mejora; devuelve true si tuvo éxito
    public static bool TryPurchase(UpgradeType t)
    {
        int lvl = GetLevel(t);
        if (lvl >= MaxUpgradeLevel) return false;
        int cost = GetConfig(t).costs[lvl];
        if (!SpendCoins(cost)) return false;
        SetLevel(t, lvl + 1);
        return true;
    }

    public static UpgradeConfig GetConfig(UpgradeType t)
    {
        foreach (var cfg in Upgrades) if (cfg.type == t) return cfg;
        return Upgrades[0];
    }

    // ── Debug: resetear todo ───────────────────────────────────────────
    public static void ResetAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
