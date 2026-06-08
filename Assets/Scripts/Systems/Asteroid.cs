using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField] private float damage = 25f;
    [SerializeField] private float spinSpeed = 60f;

    private Rigidbody2D rb;
    private Camera mainCamera;
    private const float OffScreenMargin = 3f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
    }

    public void Initialize(Vector2 direction, float speed, float dmg)
    {
        damage = dmg;
        rb.linearVelocity = direction.normalized * speed;
    }

    void Update()
    {
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
        CheckOffScreen();
    }

    private void CheckOffScreen()
    {
        if (mainCamera == null) return;

        float halfH = mainCamera.orthographicSize + OffScreenMargin;
        float halfW = halfH * mainCamera.aspect + OffScreenMargin;
        Vector3 camPos = mainCamera.transform.position;
        Vector3 pos = transform.position;

        if (pos.x < camPos.x - halfW || pos.x > camPos.x + halfW ||
            pos.y < camPos.y - halfH || pos.y > camPos.y + halfH)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerStats>()?.TakeDamage(damage);
        }
        else
        {
            other.GetComponent<EnemyBase>()?.TakeDamage(damage);
        }
    }
}
