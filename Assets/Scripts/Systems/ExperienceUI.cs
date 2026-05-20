using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExperienceUI : MonoBehaviour
{
    [Header("Elementos UI")]
    [SerializeField] private Slider expSlider;
    [SerializeField] private TextMeshProUGUI levelText;

    void Start()
    {
        if (ExperienceManager.Instance != null)
        {
            // Suscribirse a los eventos del manager
            ExperienceManager.Instance.OnExpChanged += UpdateExpBar;
            ExperienceManager.Instance.OnLevelUp += UpdateLevelText;
        }

        // Inicializar
        UpdateLevelText(1);
        UpdateExpBar(0f, 100f);
    }

    void OnDestroy()
    {
        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.OnExpChanged -= UpdateExpBar;
            ExperienceManager.Instance.OnLevelUp -= UpdateLevelText;
        }
    }

    private void UpdateExpBar(float current, float max)
    {
        if (expSlider != null)
            expSlider.value = current / max;
    }

    private void UpdateLevelText(int level)
    {
        if (levelText != null)
            levelText.text = $"Nivel {level}";
    }
}