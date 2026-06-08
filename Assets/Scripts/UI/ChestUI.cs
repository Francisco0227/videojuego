using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChestUI : MonoBehaviour
{
    [SerializeField] private GameObject       panel;
    [SerializeField] private TextMeshProUGUI  coinsEarnedText;
    [SerializeField] private TextMeshProUGUI  totalCoinsText;
    [SerializeField] private Image            boxImage;

    void Awake()
    {
        if (boxImage != null) return;
        // Auto-buscar el Image en el hijo "Box" si no fue asignado en inspector
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "Box")
            {
                boxImage = child.GetComponent<Image>();
                break;
            }
        }
    }

    public void Show(int coinsEarned)
    {
        if (panel == null) return;
        panel.SetActive(true);
        PauseManager.Pause();

        if (coinsEarnedText != null)
            coinsEarnedText.text = $"+ {coinsEarned} monedas";
        if (totalCoinsText != null)
            totalCoinsText.text  = $"Total:  {PersistentData.Coins}";
    }

    public void Collect()
    {
        if (panel != null) panel.SetActive(false);
        PauseManager.Resume();
    }
}
