using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // SINGLETON
    // ─────────────────────────────────────────────

    public static DifficultyManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ─────────────────────────────────────────────
    // ESTADO
    // ─────────────────────────────────────────────

    private int currentPlayerLevel = 1;

    // Multiplicadores actuales que se aplican a los enemigos
    private float currentHealthMult = 1f;
    private float currentDamageMult = 1f;
    private float currentSpeedMult = 1f;

    // ─────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────

    void Start()
    {
        // Suscribirse al evento de subida de nivel
        if (ExperienceManager.Instance != null)
            ExperienceManager.Instance.OnLevelUp += OnPlayerLevelUp;
    }

    void OnDestroy()
    {
        if (ExperienceManager.Instance != null)
            ExperienceManager.Instance.OnLevelUp -= OnPlayerLevelUp;
    }

    // ─────────────────────────────────────────────
    // CALCULAR MULTIPLICADORES
    // ─────────────────────────────────────────────

    private void OnPlayerLevelUp(int newLevel)
    {
        currentPlayerLevel = newLevel;

        // Recalcular los multiplicadores según el nivel
        RecalculateMultipliers();

        // Aplicar a todos los enemigos que ya están en escena
        ApplyToExistingEnemies();

        Debug.Log($"Dificultad actualizada al nivel {newLevel}: " +
                  $"vida x{currentHealthMult:F2}, " +
                  $"daño x{currentDamageMult:F2}, " +
                  $"velocidad x{currentSpeedMult:F2}");
    }

    private void RecalculateMultipliers()
    {
        // Resetear a base
        currentHealthMult = 1f;
        currentDamageMult = 1f;
        currentSpeedMult = 1f;

        // Aplicar escala según rango de nivel
        // Iteramos desde nivel 2 hasta el nivel actual
        // acumulando el multiplicador de cada nivel
        for (int level = 2; level <= currentPlayerLevel; level++)
        {
            if (level <= 3)
            {
                // Niveles 1-3: sin cambio
                // No acumulamos nada
            }
            else if (level <= 6)
            {
                // Niveles 4-6
                currentHealthMult += 0.15f;
                currentDamageMult += 0.08f;
                // Velocidad sin cambio
            }
            else if (level <= 10)
            {
                // Niveles 7-10
                currentHealthMult += 0.20f;
                currentDamageMult += 0.10f;
                currentSpeedMult += 0.05f;
            }
            else
            {
                // Nivel 11+
                currentHealthMult += 0.25f;
                currentDamageMult += 0.12f;
                currentSpeedMult += 0.05f;
            }
        }
    }

    // ─────────────────────────────────────────────
    // APLICAR A ENEMIGOS
    // ─────────────────────────────────────────────

    // Aplica los multiplicadores actuales a todos los
    // enemigos que ya están en escena
    private void ApplyToExistingEnemies()
    {
        EnemyBase[] enemies = FindObjectsByType<EnemyBase>(
            FindObjectsSortMode.None
        );

        foreach (EnemyBase enemy in enemies)
        {
            if (enemy.IsAlive())
                enemy.ApplyDifficultyScale(
                    currentHealthMult,
                    currentDamageMult,
                    currentSpeedMult
                );
        }
    }

    // ─────────────────────────────────────────────
    // GETTER PÚBLICO
    // Lo usa EnemyBase al spawnearse para aplicar
    // los multiplicadores del nivel actual
    // ─────────────────────────────────────────────

    public void ApplyToEnemy(EnemyBase enemy)
    {
        if (enemy == null) return;

        enemy.ApplyDifficultyScale(
            currentHealthMult,
            currentDamageMult,
            currentSpeedMult
        );
    }
}