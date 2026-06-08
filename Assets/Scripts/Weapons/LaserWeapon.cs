using UnityEngine;
using System.Collections;
using System.Linq;

public class LaserWeapon : WeaponBase
{
    // ─────────────────────────────────────────────
    // CONFIGURACIÓN
    // ─────────────────────────────────────────────

    [Header("Configuración")]
    [SerializeField] private GameObject projectilePrefab;

    // Estadísticas actuales del arma
    // Se actualizan cada vez que sube de nivel
    private float currentDamage = 10f;
    private float currentSpeed = 8f;
    private float currentCooldown = 1f;
    private int currentProjectiles = 1;
    private bool hasPenetration = false;

    // ─────────────────────────────────────────────
    // INICIALIZACIÓN
    // ─────────────────────────────────────────────

    protected override void Awake()
    {
        base.Awake();
    }

    // ─────────────────────────────────────────────
    // LOOP DE ATAQUE
    // ─────────────────────────────────────────────

    protected override IEnumerator AttackLoop()
    {
        while (isActive)
        {
            yield return new WaitForSeconds(GetFinalCooldown(currentCooldown));
            Fire();
        }
    }

    // ─────────────────────────────────────────────
    // DISPARO
    // ─────────────────────────────────────────────

    private void Fire()
    {
        // Obtener todos los enemigos en escena
        EnemyBase[] allEnemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);

        // Si no hay enemigos no disparar
        if (allEnemies.Length == 0) return;

        // Ordenar por distancia y tomar el más cercano
        EnemyBase closestEnemy = allEnemies
            .Where(e => e.IsAlive())
            .OrderBy(e => Vector2.Distance(transform.position, e.transform.position))
            .FirstOrDefault();

        if (closestEnemy == null) return;

        // Calcular dirección hacia el enemigo
        Vector2 direction = (closestEnemy.transform.position - transform.position).normalized;

        // Disparar según cuántos proyectiles tengamos
        if (currentProjectiles == 1)
        {
            SpawnProjectile(direction);
        }
        else if (currentProjectiles == 2)
        {
            // Dos balas con un pequeño ángulo entre ellas
            SpawnProjectile(RotateVector(direction, 10f));
            SpawnProjectile(RotateVector(direction, -10f));
        }
        else if (currentProjectiles >= 3)
        {
            // Tres balas: una al centro y dos en los lados
            SpawnProjectile(direction);
            SpawnProjectile(RotateVector(direction, 15f));
            SpawnProjectile(RotateVector(direction, -15f));
        }
    }

    private void SpawnProjectile(Vector2 direction)
    {
        if (projectilePrefab == null) return;

        // Crear la bala en la posición del jugador
        GameObject bulletObj = Instantiate(
            projectilePrefab,
            transform.position,
            Quaternion.identity
        );

        // Configurar la bala con los valores actuales del arma
        Projectile bullet = bulletObj.GetComponent<Projectile>();
        if (bullet != null)
        {
            bullet.Initialize(
                GetFinalDamage(currentDamage),
                currentSpeed,
                direction,
                hasPenetration
            );
        }
    }

    // ─────────────────────────────────────────────
    // SUBIDA DE NIVEL
    // ─────────────────────────────────────────────

    protected override void OnLevelUp(int newLevel, LevelData levelData)
    {
        // Actualizar estadísticas con los datos del nuevo nivel
        currentDamage = levelData.damage;
        currentSpeed = levelData.speed;
        currentProjectiles = levelData.projectileCount;

        // El cooldown lo calculamos desde la velocidad de ataque
        // A más speed en el ScriptableObject = menos cooldown
        // Usamos una fórmula simple: cooldown = 1 / (speed / 8)
        // donde 8 es la velocidad base del nivel 1
        currentCooldown = 8f / levelData.speed;

        // Verificar si es la evolución (nivel 6)
        if (levelData.isEvolution &&
            levelData.specialAbility == "Penetracion")
        {
            hasPenetration = true;
            Debug.Log("Laser Común evolucionado: penetración activa");
        }

        Debug.Log($"Laser Común nivel {newLevel}: " +
                  $"daño={currentDamage}, " +
                  $"proyectiles={currentProjectiles}, " +
                  $"cooldown={currentCooldown:F2}s");
    }

    // ─────────────────────────────────────────────
    // UTILIDADES
    // ─────────────────────────────────────────────

    // Rotar un vector 2D un número de grados
    private Vector2 RotateVector(Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }
}