using UnityEngine;
using System;

public class ExperienceManager : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // SINGLETON
    // ─────────────────────────────────────────────

    public static ExperienceManager Instance { get; private set; }

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
    // CONFIGURACIÓN
    // ─────────────────────────────────────────────

    [Header("Configuración de Experiencia")]
    [SerializeField] private float baseExpToLevelUp = 100f;
    // Cuánto más experiencia se necesita por cada nivel
    // 1.2 significa 20% más por nivel
    [SerializeField] private float expScalingFactor = 1.2f;

    // ─────────────────────────────────────────────
    // ESTADO
    // ─────────────────────────────────────────────

    private float currentExp = 0f;
    private float expToNextLevel;
    private int currentLevel = 1;

    // ─────────────────────────────────────────────
    // EVENTOS
    // ─────────────────────────────────────────────

    // La UI escucha este para actualizar la barra
    public event Action<float, float> OnExpChanged;     // expActual, expNecesaria

    // El sistema de items escucha este para mostrar opciones
    public event Action<int> OnLevelUp;                 // nivelNuevo

    // ─────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────

    void Start()
    {
        // Calcular la experiencia necesaria para el primer nivel
        expToNextLevel = baseExpToLevelUp;
    }

    // ─────────────────────────────────────────────
    // AGREGAR EXPERIENCIA
    // Lo llaman las estrellas al ser recogidas
    // ─────────────────────────────────────────────

    public void AddExperience(float amount)
    {
        currentExp += amount;

        // Avisar a la UI que cambió la experiencia
        OnExpChanged?.Invoke(currentExp, expToNextLevel);

        // Verificar si subimos de nivel
        // Usamos while por si la experiencia es tanta
        // que sube varios niveles a la vez
        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            currentLevel++;

            // Calcular cuánta exp necesita el siguiente nivel
            expToNextLevel = CalculateExpForLevel(currentLevel);

            // Avisar que subimos de nivel
            OnLevelUp?.Invoke(currentLevel);

            Debug.Log($"Nivel {currentLevel} alcanzado. " +
                      $"Próximo nivel: {expToNextLevel} exp");
        }

        // Avisar a la UI con los valores actualizados
        OnExpChanged?.Invoke(currentExp, expToNextLevel);
    }

    // Cuánta experiencia se necesita para cada nivel
    private float CalculateExpForLevel(int level)
    {
        // Fórmula: base * factor^(nivel-1)
        // Nivel 1: 100
        // Nivel 2: 120
        // Nivel 3: 144
        // Nivel 4: 172.8...
        return baseExpToLevelUp
             * Mathf.Pow(expScalingFactor, level - 1);
    }

    // ─────────────────────────────────────────────
    // GETTERS
    // ─────────────────────────────────────────────

    public int GetCurrentLevel() => currentLevel;
    public float GetCurrentExp() => currentExp;
    public float GetExpToNextLevel() => expToNextLevel;
    public float GetExpPercent() => currentExp / expToNextLevel;
}