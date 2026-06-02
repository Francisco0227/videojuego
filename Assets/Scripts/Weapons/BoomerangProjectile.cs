using UnityEngine;

public class BoomerangProjectile : MonoBehaviour
{
    private float     damage;
    private float     speed;
    private Vector2   outDir;
    private Transform player;
    private float     outTime  = 0.7f;
    private float     elapsed  = 0f;
    private Rigidbody2D rb;

    public void Initialize(float damage, float speed, Vector2 direction, Transform player)
    {
        this.damage = damage;
        this.speed  = speed;
        this.outDir = direction.normalized;
        this.player = player;
        Destroy(gameObject, 6f);
    }

    void Start() { rb = GetComponent<Rigidbody2D>(); }

    void FixedUpdate()
    {
        if (rb == null) return;

        elapsed += Time.fixedDeltaTime;
        Vector2 moveDir;

        if (elapsed < outTime)
        {
            moveDir = outDir;
        }
        else
        {
            if (player == null) { Destroy(gameObject); return; }
            Vector2 toPlayer = (Vector2)player.position - rb.position;
            if (toPlayer.magnitude < 0.4f) { Destroy(gameObject); return; }
            moveDir = toPlayer.normalized;
        }

        rb.linearVelocity = moveDir * speed;

        // Rotate to face movement
        float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        rb.rotation = angle;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        other.GetComponent<EnemyBase>()?.TakeDamage(damage);
    }
}
