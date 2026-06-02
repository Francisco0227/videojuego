using UnityEngine;
using System;           // Necesario para usar Action (eventos)

public class PlayerStats : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // ESTADÍSTICAS BASE (configurables desde el Inspector)
    // Estos son los valores iniciales del jugador nivel 1
    // ─────────────────────────────────────────────

    [Header("Vida")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Movimiento")]
    [SerializeField] private float baseSpeed = 5f;

    [Header("Combate")]
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float baseAttackSpeed = 1f;  // Ataques por segundo

    // ─────────────────────────────────────────────
    // MODIFICADORES (los cambian las mejoras pasivas)
    // Empiezan en 0 o en 1 dependiendo de si son suma o multiplicación
    // ─────────────────────────────────────────────

    // Estos valores los van a modificar los items pasivos
    // Los guardamos separados para poder quitarlos si fuera necesario
    private float bonusSpeed = 0f;
    private float damageMultiplier = 1f;
    private float attackSpeedMultiplier = 1f;
    private float bonusMaxHealth = 0f;
    private float defenseReduction = 0f;  // 0-0.25: % de daño reducido
    private float bonusPickupRange  = 0f; // unidades extra de atracción de estrellas

    // ─────────────────────────────────────────────
    // PROPIEDADES CALCULADAS
    // Cualquier script que pregunte la velocidad real
    // obtiene baseSpeed + todos los bonos aplicados
    // ─────────────────────────────────────────────

    public float Speed        => baseSpeed + bonusSpeed;
    public float Damage       => baseDamage * damageMultiplier;
    public float PickupRange  => 1.5f + bonusPickupRange;

    // La velocidad de ataque real = base * multiplicador de pasivas
    public float AttackSpeed
        => baseAttackSpeed * attackSpeedMultiplier;

    // La vida máxima real = base + bonus de pasivas
    public float MaxHealth
        => maxHealth + bonusMaxHealth;

    // La vida actual: cuando cambia, avisamos con un evento
    public float CurrentHealth
    {
        get => currentHealth;
        private set
        {
            // Clamp: la vida nunca sube de MaxHealth ni baja de 0
            currentHealth = Mathf.Clamp(value, 0f, MaxHealth);

            // Avisamos a quien esté escuchando que la vida cambió
            // La barra de vida va a escuchar este evento
            OnHealthChanged?.Invoke(currentHealth, MaxHealth);

            // Si la vida llegó a 0, avisamos que el jugador murió
            if (currentHealth <= 0f)
                OnPlayerDied?.Invoke();
        }
    }

    // ─────────────────────────────────────────────
    // EVENTOS
    // Otros scripts se suscriben a estos para reaccionar
    // ─────────────────────────────────────────────

    // Avisa cuando la vida cambia. Manda la vida actual y la máxima
    // para que la barra sepa qué porcentaje mostrar
    public event Action<float, float> OnHealthChanged;

    // Avisa cuando el jugador muere
    public event Action OnPlayerDied;

    // ─────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────

    void Awake()
    {
        // Aplicar mejoras permanentes de la meta-progresión
        defenseReduction    = PersistentData.GetTotalBonus(UpgradeType.Defense);
        bonusMaxHealth      = PersistentData.GetTotalBonus(UpgradeType.MaxHealth);
        bonusSpeed          = PersistentData.GetTotalBonus(UpgradeType.Speed);
        bonusPickupRange    = PersistentData.GetTotalBonus(UpgradeType.PickupRange);
        damageMultiplier    = 1f + PersistentData.GetTotalBonus(UpgradeType.Damage);

        currentHealth = MaxHealth;
    }

    void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
    }

    // ─────────────────────────────────────────────
    // MÉTODOS PÚBLICOS DE COMBATE
    // Estos los llaman las colisiones con enemigos
    // ─────────────────────────────────────────────

    public void TakeDamage(float amount)
    {
        float reduced = amount * (1f - defenseReduction);
        CurrentHealth -= Mathf.Max(0f, reduced);
    }

    // Recuperar vida
    public void Heal(float amount)
    {
        CurrentHealth += Mathf.Max(0f, amount);
    }

    // Curar un porcentaje de la vida máxima
    // Lo usará el Orbe de Vida
    public void HealPercent(float percent)
    {
        // Si percent = 0.3, cura el 30% de la vida máxima
        Heal(MaxHealth * percent);
    }

    // ─────────────────────────────────────────────
    // MÉTODOS PARA MODIFICAR ATRIBUTOS
    // Los llaman los items pasivos cuando se equipan
    // ─────────────────────────────────────────────

    // Item pasivo: velocidad de movimiento
    public void AddBonusSpeed(float amount)
    {
        bonusSpeed += amount;
    }

    // Item pasivo: daño general (porcentaje)
    // Si bonus = 0.05 aumenta 5% el daño
    public void AddDamageMultiplier(float bonus)
    {
        damageMultiplier += bonus;
    }

    // Item pasivo: velocidad de ataque (porcentaje)
    public void AddAttackSpeedMultiplier(float bonus)
    {
        attackSpeedMultiplier += bonus;
    }

    // Orbe de vida: aumenta la vida máxima en porcentaje
    public void AddMaxHealthPercent(float percent)
    {
        float bonus = maxHealth * percent;
        bonusMaxHealth += bonus;

        // Al aumentar la vida máxima, también curamos al jugador
        // para que el aumento se sienta inmediato
        Heal(bonus);
    }

    // ─────────────────────────────────────────────
    // MÉTODOS DE LECTURA
    // Para que otros sistemas puedan consultar el estado
    // ─────────────────────────────────────────────

    public bool IsAlive()
    {
        return currentHealth > 0f;
    }

    // Devuelve la vida como porcentaje (0.0 a 1.0)
    // Útil para la barra de vida
    public float GetHealthPercent()
    {
        return currentHealth / MaxHealth;
    }

    // Necesario para que las armas calculen el multiplicador de daño
    public float GetBaseDamage()
    {
        return baseDamage;
    }
}