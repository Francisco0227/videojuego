using UnityEngine;
using System.Collections;

public class RandomProjectilesWeapon : WeaponBase
{
    // ─────────────────────────────────────────────
    // CONFIGURACIÓN
    // ─────────────────────────────────────────────

    [Header("Configuración")]
    [SerializeField] private GameObject explosionPrefab;

    // Estadísticas actuales
    private float currentDamage = 12f;
    private float currentArea = 2f;
    private float currentCooldown = 2f;
    private int currentExplosionCount = 3;    // Cuántas explosiones por disparo
    private bool causesBurning = false;

    // Rango dentro del cual aparecen las explosiones
    // relativo al jugador
    private float spawnRange = 5f;

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

            FireExplosions();
        }
    }

    // ─────────────────────────────────────────────
    // GENERAR EXPLOSIONES
    // ─────────────────────────────────────────────

    private void FireExplosions()
    {
        if (explosionPrefab == null) return;

        for (int i = 0; i < currentExplosionCount; i++)
        {
            // Posición aleatoria alrededor del jugador
            Vector2 randomOffset = Random.insideUnitCircle * spawnRange;
            Vector3 spawnPos = transform.position + new Vector3(
                randomOffset.x,
                randomOffset.y,
                0f
            );

            // Crear la explosión
            GameObject explosionObj = Instantiate(
                explosionPrefab,
                spawnPos,
                Quaternion.identity
            );

            // Inicializar con los valores actuales
            ExplosionEffect explosion = explosionObj
                .GetComponent<ExplosionEffect>();

            if (explosion != null)
            {
                explosion.Initialize(
                    GetFinalDamage(currentDamage),
                    currentArea,
                    causesBurning
                );
            }
        }
    }

    // ─────────────────────────────────────────────
    // SUBIDA DE NIVEL
    // ─────────────────────────────────────────────

    protected override void OnLevelUp(int newLevel, LevelData levelData)
    {
        currentDamage = levelData.damage;
        currentArea = levelData.area;
        currentCooldown = levelData.cooldown;
        currentExplosionCount = levelData.projectileCount;

        // Verificar evolución nivel 6
        if (levelData.isEvolution &&
            levelData.specialAbility == "Llamas")
        {
            causesBurning = true;
            Debug.Log("Proyectiles Aleatorios evolucionados: llamas activas");
        }

        Debug.Log($"Proyectiles Aleatorios nivel {newLevel}: " +
                  $"daño={currentDamage}, " +
                  $"área={currentArea}, " +
                  $"explosiones={currentExplosionCount}, " +
                  $"cooldown={currentCooldown}s");
    }
}