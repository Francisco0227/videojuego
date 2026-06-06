using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // La velocidad viene de PlayerStats
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private PlayerStats stats;      // Referencia a los atributos

    private Vector2 lastDirection = Vector2.right; // Dirección por defecto: derecha

    // Propiedad pública para que el laser la lea
    public Vector2 LastDirection => lastDirection;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>(); // Busca PlayerStats en el mismo objeto
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;

        // Guardar la última dirección de movimiento
        // Solo actualizamos si el jugador se está moviendo
        // así cuando se detenga mantiene la última dirección
        if (moveInput != Vector2.zero)
            lastDirection = moveInput;
            // --- Codigo de ROTACIÓN ---
            // 1. Calculamos  el ángulo en grados hacia donde se mueve el jugador
            float angle = Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;
 
            // 2. Giramos la nave (-90 grados porque la nave apunta hacia arriba)
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    void FixedUpdate()
    {
        // Si una pasiva cambia la velocidad, el movimiento
        // lo refleja automáticamente sin tocar este script
        if (stats == null) return;
        rb.MovePosition(rb.position + moveInput * stats.Speed * Time.fixedDeltaTime);
    }
}