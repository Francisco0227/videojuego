using UnityEngine;

public class Projectile : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // CONFIGURACIÓN
    // Las armas configuran estos valores al crear la bala
    // ─────────────────────────────────────────────

    private float damage;
    private float speed;
    private float lifetime = 3f;     // Segundos antes de destruirse solo
    private bool canPenetrate = false;  // Nivel 6 del Laser Común
    private bool causesBurning = false;  // Nivel 6 de Proyectiles Aleatorios
    private float burnDuration = 3f;     // Cuánto dura el fuego
    private float burnDamage = 5f;     // Daño por segundo del fuego

    private Vector2 direction;             // Hacia dónde va la bala
    private Rigidbody2D rb;

    // ─────────────────────────────────────────────
    // INICIALIZACIÓN
    // El arma llama a este método justo después de crear la bala
    // ─────────────────────────────────────────────

    public void Initialize(float damage, float speed, Vector2 direction,
                           bool penetrate = false, bool burning = false)
    {
        this.damage = damage;
        this.speed = speed;
        this.direction = direction.normalized;
        this.canPenetrate = penetrate;
        this.causesBurning = burning;

        // Destruirse automáticamente después de lifetime segundos
        // Evita que las balas vuelen infinitamente fuera de pantalla
        Destroy(gameObject, lifetime);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (rb == null) return;
        // Mover la bala en su dirección a velocidad constante
        rb.linearVelocity = direction * speed;
    }

    // ─────────────────────────────────────────────
    // COLISIONES
    // ─────────────────────────────────────────────

    void OnTriggerEnter2D(Collider2D other)
    {
        // Solo afecta a enemigos
        if (!other.CompareTag("Enemy")) return;

        // Obtener el componente EnemyBase del enemigo
        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy == null) return;

        // Aplicar daño
        enemy.TakeDamage(damage);

        // Si causa quemadura, aplicarla
        if (causesBurning)
            enemy.ApplyBurning(burnDamage, burnDuration);

        // Si no penetra, destruir la bala al impactar
        if (!canPenetrate)
            Destroy(gameObject);

        // Si penetra, la bala sigue viajando (no se destruye)
    }
}