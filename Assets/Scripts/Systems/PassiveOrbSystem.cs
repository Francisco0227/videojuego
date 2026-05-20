using UnityEngine;
using System.Collections;

public class PassiveOrbSystem : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // REFERENCIAS
    // ─────────────────────────────────────────────

    private PlayerStats playerStats;
    private ItemBag itemBag;

    // ─────────────────────────────────────────────
    // ORBE DE LA FORTUNA
    // ─────────────────────────────────────────────

    // Probabilidad acumulada de que aparezcan items del inventario
    // al subir de nivel. Empieza en 0, sube con cada nivel del orbe.
    private float fortuneBonus = 0f;

    // En nivel 3 del orbe se muestran 4 opciones en vez de 3
    private bool hasExtraOption = false;

    // Nivel actual del orbe de fortuna
    private int fortuneLevel = 0;

    // ─────────────────────────────────────────────
    // ORBE DE VIDA
    // ─────────────────────────────────────────────

    // Probabilidad base de regeneración al subir de nivel
    private float healChance = 0f;

    // Cuántos niveles de vida máxima se han aplicado ya
    // Máximo 4 según el diseño
    private int healthBoostCount = 0;
    private const int MAX_HEALTH_BOOSTS = 4;

    // Nivel actual del orbe de vida
    private int lifeOrbLevel = 0;

    // ─────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        itemBag = GetComponent<ItemBag>();

        // Suscribirse al evento de items para detectar
        // cuando se agregan o mejoran los orbes
        if (itemBag != null)
            itemBag.OnItemAdded += OnItemAdded;
    }

    void OnDestroy()
    {
        if (itemBag != null)
            itemBag.OnItemAdded -= OnItemAdded;
    }

    // ─────────────────────────────────────────────
    // DETECTAR CUANDO SE AGREGAN LOS ORBES
    // ─────────────────────────────────────────────

    private void OnItemAdded(ItemData item, int level)
    {
        if (item == null) return;

        switch (item.itemName)
        {
            case "Orbe de la Fortuna":
                OnFortuneOrbLevelUp(item, level);
                break;

            case "Orbe de Vida":
                OnLifeOrbLevelUp(item, level);
                break;
        }
    }

    // ─────────────────────────────────────────────
    // ORBE DE LA FORTUNA — LÓGICA
    // ─────────────────────────────────────────────

    private void OnFortuneOrbLevelUp(ItemData item, int level)
    {
        fortuneLevel = level;

        // Leer el bonus de probabilidad del nivel actual
        // Usamos el campo damage 
        LevelData levelData = item.levels[level - 1];
        fortuneBonus += levelData.damage;

        // En nivel 3 se activa la opción extra
        if (level >= 3)
            hasExtraOption = true;

        Debug.Log($"Orbe de la Fortuna nivel {level}: " +
                  $"bonus probabilidad={fortuneBonus:P0}, " +
                  $"opción extra={hasExtraOption}");
    }

    // ─────────────────────────────────────────────
    // ORBE DE VIDA — LÓGICA
    // ─────────────────────────────────────────────

    private void OnLifeOrbLevelUp(ItemData item, int level)
    {
        lifeOrbLevel = level;

        LevelData levelData = item.levels[level - 1];

        // Solo aplicar aumento de vida máxima si no llegamos al límite
        // El campo damage guarda el % de vida a agregar
        if (healthBoostCount < MAX_HEALTH_BOOSTS && levelData.damage > 0f)
        {
            healthBoostCount++;
            playerStats.AddMaxHealthPercent(levelData.damage);

            Debug.Log($"Orbe de Vida: vida máxima aumentada " +
                      $"({healthBoostCount}/{MAX_HEALTH_BOOSTS})");
        }

        // Actualizar la probabilidad de curación
        // El campo cooldown guarda la probabilidad base
        healChance = levelData.cooldown;

        Debug.Log($"Orbe de Vida nivel {level}: " +
                  $"prob. curación={GetModifiedHealChance():P0}");
    }

    // ─────────────────────────────────────────────
    // MÉTODO PRINCIPAL DEL ORBE DE VIDA
    // Lo llama el sistema de niveles cada vez que el jugador sube
    // ─────────────────────────────────────────────

    public void OnPlayerLevelUp()
    {
        if (lifeOrbLevel <= 0) return;

        // Calcular la probabilidad final modificada por el orbe de fortuna
        float finalChance = GetModifiedHealChance();

        // Tirar el dado
        float roll = Random.Range(0f, 1f);

        if (roll <= finalChance)
        {
            // Curar el 30% de la vida máxima
            playerStats.HealPercent(0.3f);
            Debug.Log($"Orbe de Vida: regeneración activada " +
                      $"(roll={roll:F2}, chance={finalChance:F2})");
        }
    }

    // ─────────────────────────────────────────────
    // MÉTODOS PÚBLICOS
    // Los consultan otros sistemas
    // ─────────────────────────────────────────────

    // Devuelve la probabilidad de curación modificada por el Orbe de Fortuna
    // SENTIDO ÚNICO: Orbe de Vida consulta al Orbe de Fortuna
    // El Orbe de Fortuna nunca consulta al Orbe de Vida
    public float GetModifiedHealChance()
    {
        if (lifeOrbLevel <= 0) return 0f;

        // El Orbe de Fortuna aumenta la probabilidad de curación
        // pero con un límite razonable para no romper el balance
        float bonus = fortuneBonus * 0.5f; // La mitad del bonus de fortuna
        return Mathf.Min(healChance + bonus, 0.8f); // Máximo 80%
    }

    // Devuelve la probabilidad de que aparezca un item del inventario
    // al subir de nivel. Lo consulta el sistema de selección de items.
    public float GetInventoryItemChance()
    {
        // Probabilidad base: siempre hay al menos 1 item del inventario
        // el Orbe de Fortuna aumenta las probabilidades de que
        // aparezcan MÁS items del inventario entre las opciones
        float baseChance = 0.33f; // 33% base por opción
        return Mathf.Min(baseChance + fortuneBonus, 0.8f); // Máximo 80%
    }

    // ¿Mostrar 4 opciones en vez de 3?
    public bool HasExtraLevelUpOption()
    {
        return hasExtraOption;
    }

    // ¿Tiene el orbe de fortuna activo?
    public bool HasFortuneOrb()
    {
        return fortuneLevel > 0;
    }

    // ¿Tiene el orbe de vida activo?
    public bool HasLifeOrb()
    {
        return lifeOrbLevel > 0;
    }
}