using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NeutronLaserWeapon : WeaponBase
{
    // ─────────────────────────────────────────────
    // ESTADÍSTICAS ACTUALES
    // ─────────────────────────────────────────────

    private float currentDamage = 8f;
    private float currentCooldown = 3f;
    private float currentChainSpeed = 5f;   // Velocidad visual del salto
    private int currentChainCount = 5;    // Enemigos en cadena
    private bool hasCrossBlast = false; // Nivel 6

    // Contador de disparos para el nivel 6
    private int shotCounter = 0;

    // Referencia al movimiento del jugador para saber la dirección
    private PlayerMovement playerMovement;

    [Header("Configuración")]
    [SerializeField] private GameObject chainProjectilePrefab;

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

            shotCounter++;

            // Nivel 6: cada 6to disparo lanza rayos en cruz
            if (hasCrossBlast && shotCounter % 6 == 0)
                FireCrossBlast();
            else
                FireChain(GetAimDirection());
        }
    }

    // ─────────────────────────────────────────────
    // DISPARO NORMAL: CADENA
    // ─────────────────────────────────────────────

    private void FireChain(Vector2 aimDirection)
    {
        // Obtener todos los enemigos vivos
        EnemyBase[] allEnemies = FindObjectsByType<EnemyBase>(
            FindObjectsSortMode.None
        ).Where(e => e.IsAlive()).ToArray();

        if (allEnemies.Length == 0) return;

        // El primer objetivo es el enemigo más cercano
        // en la dirección que el jugador mira
        EnemyBase firstTarget = GetFirstTargetInDirection(
            allEnemies,
            aimDirection
        );

        if (firstTarget == null) return;

        // Obtener la cadena completa de objetivos
        List<EnemyBase> chain = GetChainTargets(
            firstTarget,
            allEnemies,
            currentChainCount
        );

        // Aplicar daño a cada enemigo en la cadena
        StartCoroutine(ApplyChainDamage(chain));
    }

    // Disparo en cruz del nivel 6
    private void FireCrossBlast()
    {
        Debug.Log("Laser de Neutrones: disparo en cruz");

        Vector2[] directions = new Vector2[]
        {
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right
        };

        foreach (Vector2 dir in directions)
            FireChain(dir);
    }

    // ─────────────────────────────────────────────
    // ALGORITMO DE CADENA
    // ─────────────────────────────────────────────

    // Primer objetivo: el más cercano en la dirección de mirada
    private EnemyBase GetFirstTargetInDirection(
        EnemyBase[] enemies, Vector2 direction)
    {
        EnemyBase best = null;
        float bestScore = float.MinValue;

        foreach (EnemyBase enemy in enemies)
        {
            Vector2 toEnemy = (enemy.transform.position
                             - transform.position).normalized;

            // Dot product: qué tan alineado está el enemigo
            // con la dirección de disparo
            // 1.0 = perfectamente alineado, -1.0 = opuesto
            float dot = Vector2.Dot(direction, toEnemy);

            // Solo considerar enemigos en el semicírculo frontal
            if (dot < 0f) continue;

            // Combinar alineación y distancia
            // Preferimos enemigos alineados y cercanos
            float distance = Vector2.Distance(
                transform.position,
                enemy.transform.position
            );
            float score = dot - (distance * 0.1f);

            if (score > bestScore)
            {
                bestScore = score;
                best = enemy;
            }
        }

        // Si no encontró nadie en el semicírculo frontal
        // tomar el más cercano sin importar dirección
        if (best == null)
        {
            best = enemies
                .OrderBy(e => Vector2.Distance(
                    transform.position,
                    e.transform.position))
                .FirstOrDefault();
        }

        return best;
    }

    // Construir la cadena saltando al más cercano
    private List<EnemyBase> GetChainTargets(
        EnemyBase firstTarget,
        EnemyBase[] allEnemies,
        int maxTargets)
    {
        List<EnemyBase> chain = new List<EnemyBase>();
        chain.Add(firstTarget);

        EnemyBase current = firstTarget;

        for (int i = 1; i < maxTargets; i++)
        {
            // Buscar el más cercano al enemigo actual
            // que no esté ya en la cadena
            EnemyBase next = allEnemies
                .Where(e => e.IsAlive() && !chain.Contains(e))
                .OrderBy(e => Vector2.Distance(
                    current.transform.position,
                    e.transform.position))
                .FirstOrDefault();

            if (next == null) break;

            chain.Add(next);
            current = next;
        }

        return chain;
    }

    // Aplicar daño con un pequeño delay entre saltos
    // para que se sienta como un rayo que viaja
    private IEnumerator ApplyChainDamage(List<EnemyBase> chain)
    {
        float delayBetweenHits = 1f / Mathf.Max(1f, currentChainSpeed);

        // El primer proyectil sale desde el jugador
        Vector3 previousPosition = transform.position;

        foreach (EnemyBase enemy in chain)
        {
            if (enemy == null || !enemy.IsAlive()) continue;

            // Lanzar una bolita desde la posición anterior
            // hacia este enemigo
            if (chainProjectilePrefab != null)
            {
                StartCoroutine(MoveProjectileToTarget(
                    previousPosition,
                    enemy,
                    currentChainSpeed
                ));
            }

            // Guardar la posición de este enemigo para el siguiente salto
            previousPosition = enemy.transform.position;

            // Esperar a que la bolita llegue antes del siguiente salto
            yield return new WaitForSeconds(delayBetweenHits);

            // Aplicar daño cuando la bolita llega
            if (enemy == null || !enemy.IsAlive()) continue;
            enemy.TakeDamage(GetFinalDamage(currentDamage));
        }
    }


    // ─────────────────────────────────────────────
    // DIRECCIÓN DE APUNTADO
    // ─────────────────────────────────────────────

    private Vector2 GetAimDirection()
    {
        if (playerMovement != null)
            return playerMovement.LastDirection;

        // Fallback: apuntar al enemigo más cercano
        EnemyBase nearest = FindObjectsByType<EnemyBase>(
            FindObjectsSortMode.None)
            .Where(e => e.IsAlive())
            .OrderBy(e => Vector2.Distance(
                transform.position,
                e.transform.position))
            .FirstOrDefault();

        if (nearest != null)
            return (nearest.transform.position
                  - transform.position).normalized;

        return Vector2.right;
    }

    // ─────────────────────────────────────────────
    // SUBIDA DE NIVEL
    // ─────────────────────────────────────────────

    protected override void OnLevelUp(int newLevel, LevelData levelData)
    {
        currentDamage = levelData.damage;
        currentChainSpeed = levelData.speed;
        currentChainCount = levelData.projectileCount;
        currentCooldown = levelData.cooldown;

        if (levelData.isEvolution &&
            levelData.specialAbility == "RayosCruzados")
        {
            hasCrossBlast = true;
            Debug.Log("Laser de Neutrones evolucionado: rayos cruzados activos");
        }

        Debug.Log($"Laser de Neutrones nivel {newLevel}: " +
                  $"daño={currentDamage}, " +
                  $"cadena={currentChainCount} enemigos, " +
                  $"cooldown={currentCooldown}s");
    }

    private IEnumerator MoveProjectileToTarget(
        Vector3 startPos,
        EnemyBase target,
        float speed)
    {
        // Crear la bolita en la posición de origen
        GameObject projObj = Instantiate(
            chainProjectilePrefab,
            startPos,
            Quaternion.identity
        );

        // Mover la bolita hacia el enemigo frame a frame
        while (projObj != null && target != null)
        {
            // Mover hacia el enemigo
            projObj.transform.position = Vector3.MoveTowards(
                projObj.transform.position,
                target.transform.position,
                speed * Time.deltaTime
            );

            // Si llegó al enemigo destruirla
            float distToTarget = Vector3.Distance(
                projObj.transform.position,
                target.transform.position
            );

            if (distToTarget < 0.1f)
            {
                Destroy(projObj);
                yield break;
            }

            yield return null; // Esperar al siguiente frame
        }

        // Si el enemigo murió antes de que llegara, destruir igual
        if (projObj != null)
            Destroy(projObj);
    }
}