using UnityEngine;
using System.Collections;
using System.Linq;

public class AstronautWeapon : WeaponBase
{
    // ─────────────────────────────────────────────
    // CONFIGURACIÓN
    // ─────────────────────────────────────────────

    [Header("Configuración")]
    [SerializeField] private GameObject astronautPrefab;

    // Estadísticas actuales
    private float currentDamage = 20f;
    private float currentSpeed = 6f;
    private float currentRange = 3f;
    private float currentCooldown = 1.5f;
    private int currentProjectiles = 1;
    private bool hasSaberAttack = false;

    // Referencia al movimiento para saber la dirección
    private PlayerMovement playerMovement;

    // ─────────────────────────────────────────────
    // INICIALIZACIÓN
    // ─────────────────────────────────────────────

    protected override void Awake()
    {
        base.Awake();
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    // ─────────────────────────────────────────────
    // LOOP DE ATAQUE
    // ─────────────────────────────────────────────

    protected override IEnumerator AttackLoop()
    {
        while (isActive)
        {
            yield return new WaitForSeconds(
                GetFinalCooldown(currentCooldown)
            );

            Launch();
        }
    }

    // ─────────────────────────────────────────────
    // LANZAR ASTRONAUTA
    // ─────────────────────────────────────────────

    private void Launch()
    {
        if (astronautPrefab == null) return;

        // Dirección principal: hacia el enemigo más cercano
        Vector2 mainDirection = GetLaunchDirection();

        if (currentProjectiles == 1)
        {
            SpawnAstronaut(mainDirection);
        }
        else
        {
            // Nivel 3+: lanzar en direcciones opuestas
            // horizontal: uno al frente y uno atrás
            SpawnAstronaut(mainDirection);
            SpawnAstronaut(-mainDirection);
        }
    }

    private void SpawnAstronaut(Vector2 direction)
    {
        GameObject obj = Instantiate(
            astronautPrefab,
            transform.position,
            Quaternion.identity
        );

        AstronautProjectile astronaut = obj
            .GetComponent<AstronautProjectile>();

        if (astronaut != null)
        {
            astronaut.Initialize(
                damage: GetFinalDamage(currentDamage),
                speed: currentSpeed,
                maxRange: currentRange,
                direction: direction,
                saberAttack: hasSaberAttack,
                saberDmg: GetFinalDamage(currentDamage) * 1.5f
            );
        }
    }

    // ─────────────────────────────────────────────
    // DIRECCIÓN DE LANZAMIENTO
    // ─────────────────────────────────────────────

    private Vector2 GetLaunchDirection()
    {
        // Buscar el enemigo más cercano
        EnemyBase nearest = FindObjectsByType<EnemyBase>(
            FindObjectsSortMode.None)
            .Where(e => e.IsAlive())
            .OrderBy(e => Vector2.Distance(
                transform.position,
                e.transform.position))
            .FirstOrDefault();

        // Si hay enemigo apuntar hacia él
        if (nearest != null)
            return (nearest.transform.position
                  - transform.position).normalized;

        // Si no hay enemigos usar la dirección de movimiento
        if (playerMovement != null)
            return playerMovement.LastDirection;

        return Vector2.right;
    }

    // ─────────────────────────────────────────────
    // SUBIDA DE NIVEL
    // ─────────────────────────────────────────────

    protected override void OnLevelUp(int newLevel, LevelData levelData)
    {
        currentDamage = levelData.damage;
        currentRange = levelData.area;
        currentSpeed = levelData.speed;
        currentCooldown = 8f / levelData.speed;
        currentProjectiles = levelData.projectileCount;

        if (levelData.isEvolution &&
            levelData.specialAbility == "SableDeLuz")
        {
            hasSaberAttack = true;
            Debug.Log("Astronauta evolucionado: sable de luz activo");
        }

        Debug.Log($"Astronauta Atacante nivel {newLevel}: " +
                  $"daño={currentDamage}, " +
                  $"rango={currentRange}, " +
                  $"proyectiles={currentProjectiles}");
    }
}