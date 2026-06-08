using UnityEngine;

// Enum = lista de opciones fijas
// Usamos enums para categorizar items sin usar strings
// (los strings son propensos a errores de tipeo)
public enum ItemType
{
    Active,     // Armas y habilidades activas
    Passive     // Mejoras pasivas
}

// Con este atributo, en Unity puedes hacer:
// Clic derecho en Project → Create → Items → Item Data
[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Item Data")]
public class ItemData : ScriptableObject
{
    // ─────────────────────────────────────────────
    // INFORMACIÓN GENERAL
    // ─────────────────────────────────────────────

    [Header("Información General")]
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;
    public ItemType itemType;
    public int maxLevel = 6;

    // ─────────────────────────────────────────────
    // ESTADÍSTICAS POR NIVEL
    // Cada índice del array corresponde a un nivel
    // levels[0] = nivel 1, levels[1] = nivel 2, etc.
    // ─────────────────────────────────────────────

    [Header("Estadísticas por Nivel")]
    public LevelData[] levels;
}

// Esta clase guarda los datos de un nivel específico del item
// [System.Serializable] hace que Unity pueda mostrarlo en el Inspector
[System.Serializable]
public class LevelData
{
    [TextArea] public string levelDescription;  // Qué cambia en este nivel

    // Estadísticas genéricas que cada item usará según necesite
    // No todos los items usan todos estos valores
    // pero tenerlos aquí nos evita crear una clase por cada item
    public float damage;
    public float area;
    public float speed;
    public float duration;
    public float cooldown;
    public int projectileCount;
    public bool isEvolution;           // ¿Es el nivel 6 (evolución)?
    public string specialAbility;       // Descripción de la habilidad especial
}