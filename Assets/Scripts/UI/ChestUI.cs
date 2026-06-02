using UnityEngine;
using TMPro;

public class ChestUI : MonoBehaviour
{
    [SerializeField] private GameObject       panel;
    [SerializeField] private TextMeshProUGUI  coinsEarnedText;
    [SerializeField] private TextMeshProUGUI  totalCoinsText;

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
