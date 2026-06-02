using UnityEngine;
using System.Collections;

public class EnemyBase : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // ESTADÍSTICAS BASE
    // Configurables desde el Inspector para crear
    // distintos tipos de enemigos cambiando estos valores
    // ─────────────────────────────────────────────

    [Header("Estadísticas")]
    [SerializeField] protected float maxHealth = 30f;
    [SerializeField] protected float currentHealth;
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float attackCooldown = 1.5f;  // Segundos entre ataques
    [SerializeField] protected float attackRange = 0.8f;  // Distancia para atacar

    // Estrellas de experiencia

    [Header("Experiencia")]
    [SerializeField] private GameObject expStarPrefab;
    [SerializeField] private float expValue = 10f;

    // ─────────────────────────────────────────────
    // ESCALADO DE DIFICULTAD
    // Estos multiplicadores los aplica el sistema
    // de dificultad cuando el jugador sube de nivel
    // ─────────────────────────────────────────────

    protected float healthMultiplier = 1f;
    protected float damageMultiplier = 1f;
    protected float speedMultiplier = 1f;

    // ─────────────────────────────────────────────
    // REFERENCIAS
    // ─────────────────────────────────────────────

    protected Transform playerTransform;
    protected PlayerStats playerStats;
    protected Rigidbody2D rb;

    // ─────────────────────────────────────────────
    // ESTADO INTERNO
    // ─────────────────────────────────────────────

    protected bool isAlive = true;
    protected bool isAttacking = false;  // Evita ataques simultáneos

    // ─────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
            playerStats = playerObject.GetComponent<PlayerStats>();
        }

        // Aplicar dificultad actual al spawnear
        // Si el jugador está en nivel 5 y spawnea un enemigo nuevo
        // ese enemigo ya nace con los stats del nivel 5
        if (DifficultyManager.Instance != null)
            DifficultyManager.Instance.ApplyToEnemy(this);

        // Inicializar vida con multiplicador ya aplicado
        currentHealth = maxHealth * healthMultiplier;

        StartCoroutine(BehaviourLoop());
    }

    protected virtual void FixedUpdate()
    {
        if (!isAlive) return;
        if (playerTransform == null) return;

        MoveTowardsPlayer();
    }

    // ─────────────────────────────────────────────
    // MOVIMIENTO
    // ─────────────────────────────────────────────

    protected virtual void MoveTowardsPlayer()
    {
        // Calculamos la distancia al jugador
        float distanceToPlayer = Vector2.Distance(
            transform.position,
            playerTransform.position
        );

        // Si estamos dentro del rango de ataque no nos movemos
        // El enemigo se detiene justo cuando puede atacar
        if (distanceToPlayer <= attackRange) return;

        // Mover hacia el jugador
        Vector2 newPosition = Vector2.MoveTowards(
            rb.position,
            playerTransform.position,
            moveSpeed * speedMultiplier * Time.fixedDeltaTime
        );

        rb.MovePosition(newPosition);
    }

    // ─────────────────────────────────────────────
    // LOOP DE COMPORTAMIENTO
    // Controla el ciclo de ataque del enemigo
    // ─────────────────────────────────────────────

    protected virtual IEnumerator BehaviourLoop()
    {
        // Espera un momento antes de empezar a atacar
        // Evita que todos los enemigos ataquen al mismo tiempo al spawnearse
        yield return new WaitForSeconds(Random.Range(0.2f, 0.8f));

        while (isAlive)
        {
            // Esperar hasta estar en rango de ataque
            yield return new WaitUntil(() =>
                playerTransform != null &&
                Vector2.Distance(transform.position, playerTransform.position) <= attackRange
            );

            // Atacar
            PerformAttack();

            // Esperar el cooldown antes del siguiente ataque
            yield return new WaitForSeconds(attackCooldown);
        }
    }

    // ─────────────────────────────────────────────
    // ATAQUE
    // ─────────────────────────────────────────────

    protected virtual void PerformAttack()
    {
        if (!isAlive) return;
        if (playerStats == null) return;

        // Verificar que seguimos en rango al momento de atacar
        float distanceToPlayer = Vector2.Distance(
            transform.position,
            playerTransform.position
        );

        if (distanceToPlayer > attackRange) return;

        // Aplicar daño al jugador
        float finalDamage = damage * damageMultiplier;
        playerStats.TakeDamage(finalDamage);

        Debug.Log($"Enemigo atacó al jugador por {finalDamage} de daño");
    }

    // ─────────────────────────────────────────────
    // RECIBIR DAÑO
    // ─────────────────────────────────────────────

    public virtual void TakeDamage(float amount)
    {
        if (!isAlive) return;

        currentHealth -= amount;

        if (currentHealth <= 0f)
            Die();
    }

    // ─────────────────────────────────────────────
    // MUERTE
    // ─────────────────────────────────────────────
    protected virtual void Die()
    {
        isAlive = false;
        StopAllCoroutines();

        ScoreManager.Instance?.RegisterKill();
        DropExperienceStar();

        Destroy(gameObject);
    }

    private void DropExperienceStar()
    {
        if (expStarPrefab == null) return;

        // Crear la estrella en la posición donde murió el enemigo
        GameObject starObj = Instantiate(
            expStarPrefab,
            transform.position,
            Quaternion.identity
        );

        // Configurar el valor de experiencia
        ExperienceStar star = starObj.GetComponent<ExperienceStar>();
        if (star != null)
            star.Initialize(expValue);
    }



    // ─────────────────────────────────────────────
    // ESCALADO DE DIFICULTAD
    // Lo llama el sistema de dificultad cuando el jugador sube de nivel
    // ─────────────────────────────────────────────

    public void ApplyDifficultyScale(float hpMult, float dmgMult, float spdMult)
    {
        healthMultiplier = hpMult;
        damageMultiplier = dmgMult;
        speedMultiplier = spdMult;

        // Actualizar la vida actual con el nuevo multiplicador
        float newMaxHealth = maxHealth * healthMultiplier;
        // Mantener el porcentaje de vida actual
        float healthPercent = currentHealth / (maxHealth * (healthMultiplier / hpMult));
        currentHealth = newMaxHealth * healthPercent;
    }

    // ─────────────────────────────────────────────
    // GETTERS ÚTILES
    // ─────────────────────────────────────────────

    public bool IsAlive() => isAlive;

    public float GetHealthPercent() => currentHealth / (maxHealth * healthMultiplier);

    // ─────────────────────────────────────────────
    // EFECTOS DE ESTADO
    // ─────────────────────────────────────────────

    // Llamado por proyectiles con habilidad especial Llamas
    public void ApplyBurning(float damagePerSecond, float duration)
    {
        // Si ya está ardiendo, reiniciar el timer
        StopCoroutine("BurnCoroutine");
        StartCoroutine(BurnCoroutine(damagePerSecond, duration));
    }

    private IEnumerator BurnCoroutine(float damagePerSecond, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration && isAlive)
        {
            yield return new WaitForSeconds(1f);
            elapsed += 1f;

            // Daño por segundo mientras dure el fuego
            TakeDamage(damagePerSecond);
        }
    }

    // Llamado por el Campo de Fuerza nivel 6
    public void ApplySlow(float slowMultiplier)
    {
        // slowMultiplier = 0.5 significa 50% de velocidad
        speedMultiplier = slowMultiplier;
    }

    // Llamado cuando el enemigo sale del Campo de Fuerza
    public void RemoveSlow()
    {
        speedMultiplier = 1f;
    }
}