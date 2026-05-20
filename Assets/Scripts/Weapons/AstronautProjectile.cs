using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AstronautProjectile : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // ESTADO
    // ─────────────────────────────────────────────

    private enum AstronautState { GoingOut, ComingBack }
    private AstronautState state = AstronautState.GoingOut;

    // ─────────────────────────────────────────────
    // CONFIGURACIÓN
    // La inicializa AstronautWeapon al crear el proyectil
    // ─────────────────────────────────────────────

    private float damage;
    private float speed;
    private float maxRange;
    private bool hasSaberAttack = false;  // Nivel 6
    private float saberDamage;

    private Vector3 startPosition;
    private Vector3 targetPosition; // Punto máximo de viaje

    // Enemigos ya golpeados en este viaje de ida
    // Para no golpear al mismo enemigo dos veces seguidas
    private List<EnemyBase> hitOnWayOut = new List<EnemyBase>();
    private bool didSaberHit = false;

    // ─────────────────────────────────────────────
    // INICIALIZACIÓN
    // ─────────────────────────────────────────────

    public void Initialize(float damage, float speed, float maxRange,
                           Vector2 direction, bool saberAttack = false,
                           float saberDmg = 0f)
    {
        this.damage = damage;
        this.speed = speed;
        this.maxRange = maxRange;
        this.hasSaberAttack = saberAttack;
        this.saberDamage = saberDmg;

        startPosition = transform.position;
        targetPosition = startPosition + new Vector3(
            direction.x,
            direction.y,
            0f
        ) * maxRange;

        // Rotar el proyectil en la dirección de viaje
        float angle = Mathf.Atan2(direction.y, direction.x)
                    * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    // ─────────────────────────────────────────────
    // MOVIMIENTO
    // ─────────────────────────────────────────────

    void Update()
    {
        switch (state)
        {
            case AstronautState.GoingOut:
                MoveTowardsTarget();
                break;

            case AstronautState.ComingBack:
                MoveTowardsStart();
                break;
        }
    }

    private void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        // Llegó al punto máximo
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            // Ataque extra del sable al llegar al rango máximo
            if (hasSaberAttack && !didSaberHit)
            {
                didSaberHit = true;
                SaberAttack();
            }

            // Cambiar a estado de regreso
            state = AstronautState.ComingBack;

            // Limpiar lista de hits para poder golpear
            // a los mismos enemigos en el camino de vuelta
            hitOnWayOut.Clear();
        }
    }

    private void MoveTowardsStart()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            startPosition,
            speed * Time.deltaTime
        );

        // Llegó de vuelta al jugador: destruirse
        if (Vector3.Distance(transform.position, startPosition) < 0.1f)
            Destroy(gameObject);
    }

    // ─────────────────────────────────────────────
    // COLISIONES
    // ─────────────────────────────────────────────

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy == null || !enemy.IsAlive()) return;

        // Evitar golpear al mismo enemigo dos veces en la misma dirección
        if (state == AstronautState.GoingOut)
        {
            if (hitOnWayOut.Contains(enemy)) return;
            hitOnWayOut.Add(enemy);
        }

        enemy.TakeDamage(damage);
    }

    // ─────────────────────────────────────────────
    // ATAQUE ESPECIAL NIVEL 6
    // ─────────────────────────────────────────────

    private void SaberAttack()
    {
        // Daño en área al llegar al punto máximo
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            1.5f    // Radio del ataque del sable
        );

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy == null || !enemy.IsAlive()) continue;

            enemy.TakeDamage(saberDamage);
            Debug.Log($"Sable de luz: golpe extra por {saberDamage}");
        }
    }
}