using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // CONFIGURACIÓN
    // La inicializa RandomProjectilesWeapon al crearlo
    // ─────────────────────────────────────────────

    private float damage;
    private float area;
    private float duration = 0.3f;  // Cuánto dura visible la explosión
    private bool causesBurning = false;
    private float burnDamage = 5f;
    private float burnDuration = 3f;

    public void Initialize(float damage, float area,
                           bool burning = false,
                           float burnDmg = 5f, float burnDur = 3f)
    {
        this.damage = damage;
        this.area = area;
        this.causesBurning = burning;
        this.burnDamage = burnDmg;
        this.burnDuration = burnDur;

        // Ajustar el tamaño visual al área
        transform.localScale = Vector3.one * area * 0.5f;

        // Aplicar daño inmediatamente al aparecer
        DamageEnemiesInArea();

        // Destruirse después de la duración
        Destroy(gameObject, duration);
    }

    private void DamageEnemiesInArea()
    {
        // Detectar todos los colliders en el área de explosión
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            area * 0.5f
        );

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy == null || !enemy.IsAlive()) continue;

            // Aplicar daño
            enemy.TakeDamage(damage);

            // Aplicar quemadura si tenemos el nivel 6
            if (causesBurning)
                enemy.ApplyBurning(burnDamage, burnDuration);
        }
    }
}