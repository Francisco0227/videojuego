using UnityEngine;
using System.Collections;

public abstract class WeaponBase : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // REFERENCIAS
    // ─────────────────────────────────────────────

    protected PlayerStats playerStats;
    protected ItemBag itemBag;
    [SerializeField] protected ItemData itemData;   // Los datos de este arma en el ScriptableObject

    // ─────────────────────────────────────────────
    // ESTADO DEL ARMA
    // ─────────────────────────────────────────────

    protected int currentLevel = 0;   // 0 = arma no desbloqueada
    protected bool isActive = false;

    // ─────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────

    protected virtual void Start()
    {
        playerStats = GetComponentInParent<PlayerStats>();
        itemBag = GetComponentInParent<ItemBag>();

        // Suscribirse al evento de items para saber cuando este arma sube de nivel
        if (itemBag != null)
            itemBag.OnItemAdded += OnItemLevelChanged;
    }

    protected virtual void OnDestroy()
    {
        if (itemBag != null)
            itemBag.OnItemAdded -= OnItemLevelChanged;
    }

    // ─────────────────────────────────────────────
    // MÉTODOS QUE CADA ARMA DEBE IMPLEMENTAR
    // ─────────────────────────────────────────────

    // Cada arma define cómo ataca
    protected abstract IEnumerator AttackLoop();

    // Cada arma define qué hace al subir de nivel
    protected abstract void OnLevelUp(int newLevel, LevelData levelData);

    // ─────────────────────────────────────────────
    // LÓGICA COMÚN
    // ─────────────────────────────────────────────

    // Se llama cuando cualquier item sube de nivel
    // Cada arma verifica si el item que subió es el suyo
    private void OnItemLevelChanged(ItemData item, int level)
    {
        // Verificar que el item que subió es el de esta arma
        if (item != itemData) return;

        currentLevel = level;

        if (!isActive)
        {
            // Primera vez que se activa este arma
            isActive = true;
            StartCoroutine(AttackLoop());
        }

        // Notificar a la subclase que subió de nivel
        LevelData levelData = item.levels[level - 1];
        OnLevelUp(level, levelData);
    }

    // Calcula el daño final aplicando modificadores del jugador
    protected float GetFinalDamage(float baseDamage)
    {
        if (playerStats == null) return baseDamage;
        // damageMultiplier está en PlayerStats como propiedad
        // Damage ya devuelve baseDamage * multiplicador
        // así que solo necesitamos el multiplicador
        return baseDamage * (playerStats.Damage / Mathf.Max(1f, playerStats.GetBaseDamage()));
    }

    protected float GetFinalCooldown(float baseCooldown)
    {
        if (playerStats == null) return baseCooldown;
        // AttackSpeed empieza en 1.0 y sube con las pasivas
        // Dividir el cooldown entre AttackSpeed lo hace más corto
        return baseCooldown / Mathf.Max(0.1f, playerStats.AttackSpeed);
    }
}