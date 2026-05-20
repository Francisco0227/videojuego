using UnityEngine;

public class ExperienceStar : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // CONFIGURACIÓN
    // ─────────────────────────────────────────────

    [SerializeField] private float expValue = 10f;  // Cuánta exp da
    [SerializeField] private float pickupRadius = 1.5f; // Distancia para recoger
    [SerializeField] private float lifetime = 100f;  // Desaparece si nadie la recoge
    [SerializeField] private float attractSpeed = 5f;   // Velocidad al acercarse al jugador

    // ─────────────────────────────────────────────
    // ESTADO
    // ─────────────────────────────────────────────

    private Transform playerTransform;
    private bool isAttracting = false; // ¿Está viajando hacia el jugador?

    // ─────────────────────────────────────────────
    // INICIALIZACIÓN
    // Las estrellas se pueden configurar al crearlas
    // para diferentes valores según el tipo de enemigo
    // ─────────────────────────────────────────────

    public void Initialize(float value)
    {
        expValue = value;
    }

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        // Destruirse automáticamente si nadie la recoge
        Destroy(gameObject, lifetime);
    }

    // ─────────────────────────────────────────────
    // UPDATE: DETECCIÓN Y ATRACCIÓN
    // ─────────────────────────────────────────────

    void Update()
    {
        if (playerTransform == null) return;

        float distToPlayer = Vector2.Distance(
            transform.position,
            playerTransform.position
        );

        // Si el jugador está cerca, empezar a atraerse
        if (distToPlayer <= pickupRadius)
            isAttracting = true;

        // Mover hacia el jugador si está siendo atraída
        if (isAttracting)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                playerTransform.position,
                attractSpeed * Time.deltaTime
            );
        }
    }

    // ─────────────────────────────────────────────
    // RECOLECCIÓN
    // ─────────────────────────────────────────────

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Dar experiencia al manager
        if (ExperienceManager.Instance != null)
            ExperienceManager.Instance.AddExperience(expValue);

        // Destruirse al ser recogida
        Destroy(gameObject);
    }
}