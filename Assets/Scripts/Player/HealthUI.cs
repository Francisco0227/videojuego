using UnityEngine;
using UnityEngine.UI;       // Para usar Slider
using TMPro;                // Para usar TextMeshPro

public class HealthUI : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // REFERENCIAS A LA UI
    // Las asignaremos desde el Inspector arrastrando
    // ─────────────────────────────────────────────
    [Header("Elementos UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Colores de la barra")]
    [SerializeField] private Color colorHighHealth = new Color(0.86f, 0.20f, 0.20f); // Rojo
    [SerializeField] private Color colorMidHealth = new Color(0.86f, 0.60f, 0.10f); // Naranja
    [SerializeField] private Color colorLowHealth = new Color(0.86f, 0.86f, 0.10f); // Amarillo

    // Referencia a la imagen de la barra para cambiarle el color
    private Image fillImage;

    // Referencia al jugador
    private PlayerStats playerStats;

    // ─────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────

    void Start()
    {
        // Buscamos al jugador en la escena por su tag
        // En el siguiente paso le asignaremos el tag "Player" al jugador
        GameObject playerObject = GameObject.FindWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogError("HealthUI: No encontré un objeto con tag 'Player'");
            return;
        }

        // Obtenemos el componente PlayerStats del jugador
        playerStats = playerObject.GetComponent<PlayerStats>();

        if (playerStats == null)
        {
            Debug.LogError("HealthUI: El jugador no tiene PlayerStats");
            return;
        }

        // Nos suscribimos al evento OnHealthChanged de PlayerStats
        // Cada vez que la vida cambie, se llamará a nuestro método UpdateHealthUI
        playerStats.OnHealthChanged += UpdateHealthUI;

        // Obtenemos la imagen del Fill para cambiar su color
        if (healthSlider != null)
            fillImage = healthSlider.fillRect.GetComponent<Image>();

        // Inicializamos la UI con los valores actuales
        // Llamamos directamente con los valores iniciales
        UpdateHealthUI(playerStats.CurrentHealth, playerStats.MaxHealth);
    }

    void OnDestroy()
    {
        // Buena práctica: nos desuscribimos cuando este objeto se destruye
        // Evita errores si el jugador muere antes que la UI
        if (playerStats != null)
            playerStats.OnHealthChanged -= UpdateHealthUI;
    }

    // ─────────────────────────────────────────────
    // ACTUALIZAR LA UI
    // Este método se llama automáticamente cada vez
    // que PlayerStats dispara el evento OnHealthChanged
    // ─────────────────────────────────────────────

    private void UpdateHealthUI(float current, float max)
    {
        // Calculamos el porcentaje de vida (0.0 a 1.0)
        float percent = (max > 0f) ? current / max : 0f;

        // Actualizar el Slider
        if (healthSlider != null)
            healthSlider.value = percent;

        // Actualizar el texto: "85 / 100"
        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";

        // Cambiar el color según el porcentaje de vida
        UpdateBarColor(percent);
    }

    private void UpdateBarColor(float percent)
    {
        if (fillImage == null) return;

        // Más del 60%: rojo (salud normal)
        // Entre 30% y 60%: naranja (precaución)
        // Menos del 30%: amarillo (peligro)
        if (percent > 0.6f)
            fillImage.color = colorHighHealth;
        else if (percent > 0.3f)
            fillImage.color = colorMidHealth;
        else
            fillImage.color = colorLowHealth;
    }
}