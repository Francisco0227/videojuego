using UnityEngine;
using TMPro;

public class GameTimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    void Update()
    {
        if (ScoreManager.Instance == null || timerText == null) return;
        float t   = ScoreManager.Instance.TimeAlive;
        int   min = (int)(t / 60);
        int   sec = (int)(t % 60);
        timerText.text = $"{min:00}:{sec:00}";
    }
}
