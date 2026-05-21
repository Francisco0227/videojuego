using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ForceFieldWeapon : WeaponBase
{
    // ─────────────────────────────────────────────
    // ESTADÍSTICAS ACTUALES
    // ─────────────────────────────────────────────

    private float currentDamage = 5f;
    private float currentArea = 2f;
    private float damageInterval = 0.5f;  // Daño cada medio segundo
    private bool hasSlowEffect = false; // Nivel 6
    private float slowMultiplier = 0.4f;  // 40% de velocidad al ralentizar

    // ─────────────────────────────────────────────
    // COMPONENTES
    // ─────────────────────────────────────────────

    private CircleCollider2D fieldCollider;

    // Timer para el daño continuo
    private float damageTimer = 0f;

    // Lista de enemigos dentro del campo ahora mismo
    // La necesitamos para aplicar y quitar la ralentización
    private List<EnemyBase> enemiesInField = new List<EnemyBase>();

    // ─────────────────────────────────────────────
    // INICIALIZACIÓN
    // ─────────────────────────────────────────────

    protected override void Awake()
    {
        base.Awake();

        // Obtener o crear el CircleCollider2D
        fieldCollider = GetComponent<CircleCollider2D>();
        if (fieldCollider == null)
            fieldCollider = gameObject.AddComponent<CircleCollider2D>();

        // El collider del campo de fuerza es un trigger
        fieldCollider.isTrigger = true;
        fieldCollider.radius = currentArea;
    }

    // ─────────────────────────────────────────────
    // UPDATE: DAÑO CONTINUO
    // ─────────────────────────────────────────────

    void Update()
    {
        if (!isActive) return;
        if (enemiesInField.Count == 0) return;

        // Acumular tiempo
        damageTimer += Time.deltaTime;

        // Aplicar daño cuando se cumple el intervalo
        if (damageTimer >= damageInterval)
        {
            damageTimer = 0f;
            DamageEnemiesInField();
        }
    }

    private void DamageEnemiesInField()
    {
        // Iterar hacia atrás para poder quitar elementos
        // sin romper el loop si un enemigo muere
        for (int i = enemiesInField.Count - 1; i >= 0; i--)
        {
            EnemyBase enemy = enemiesInField[i];

            // Si el enemigo murió o fue destruido, quitarlo de la lista
            if (enemy == null || !enemy.IsAlive())
            {
                enemiesInField.RemoveAt(i);
                continue;
            }

            // Aplicar daño
            enemy.TakeDamage(GetFinalDamage(currentDamage));
        }
    }

    // ─────────────────────────────────────────────
    // DETECCIÓN DE ENEMIGOS
    // ─────────────────────────────────────────────

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        if (!other.CompareTag("Enemy")) return;

        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy == null) return;

        // Agregar a la lista si no está ya
        if (!enemiesInField.Contains(enemy))
            enemiesInField.Add(enemy);

        // Aplicar ralentización si tenemos el nivel 6
        if (hasSlowEffect)
            enemy.ApplySlow(slowMultiplier);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy == null) return;

        // Quitar de la lista
        enemiesInField.Remove(enemy);

        // Quitar ralentización al salir
        if (hasSlowEffect)
            enemy.RemoveSlow();
    }

    // ─────────────────────────────────────────────
    // LOOP DE ATAQUE
    // El campo de fuerza no usa AttackLoop porque
    // su daño se maneja en Update con el timer
    // Pero WeaponBase lo requiere así que lo dejamos vacío
    // ─────────────────────────────────────────────

    protected override IEnumerator AttackLoop()
    {
        // El campo de fuerza no necesita un loop de ataque
        // su lógica está en Update y OnTriggerStay2D
        yield return null;
    }

    // ─────────────────────────────────────────────
    // SUBIDA DE NIVEL
    // ─────────────────────────────────────────────

    protected override void OnLevelUp(int newLevel, LevelData levelData)
    {
        currentDamage = levelData.damage;
        currentArea = levelData.area;

        // Actualizar el radio del collider
        if (fieldCollider != null)
            fieldCollider.radius = currentArea;

        // Verificar evolución nivel 6
        if (levelData.isEvolution &&
            levelData.specialAbility == "Ralentizacion")
        {
            hasSlowEffect = true;

            // Aplicar ralentización a enemigos que ya están dentro
            foreach (EnemyBase enemy in enemiesInField)
            {
                if (enemy != null && enemy.IsAlive())
                    enemy.ApplySlow(slowMultiplier);
            }

            Debug.Log("Campo de Fuerza evolucionado: ralentización activa");
        }

        Debug.Log($"Campo de Fuerza nivel {newLevel}: " +
                  $"daño={currentDamage}, " +
                  $"área={currentArea}");
    }
}