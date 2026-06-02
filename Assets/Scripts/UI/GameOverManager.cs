using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private GameObject        gameOverPanel;
    [SerializeField] private TextMeshProUGUI   killsText;
    [SerializeField] private TextMeshProUGUI   timeText;
    [SerializeField] private TextMeshProUGUI   scoreText;

    private PlayerStats playerStats;

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
                playerStats.OnPlayerDied += ShowGameOver;
        }
    }

    void OnDestroy()
    {
        if (playerStats != null)
            playerStats.OnPlayerDied -= ShowGameOver;
    }

    private void ShowGameOver()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.StopTracking();

        if (ScoreManager.Instance != null)
        {
            int   kills = ScoreManager.Instance.EnemiesKilled;
            float time  = ScoreManager.Instance.TimeAlive;
            int   score = ScoreManager.Instance.Score;
            int   min   = (int)(time / 60);
            int   sec   = (int)(time % 60);

            if (killsText != null)
                killsText.text = $"Enemigos eliminados:  {kills}  × 100  =  {kills * 100} pts";
            if (timeText != null)
                timeText.text  = $"Tiempo sobrevivido:   {min:00}:{sec:00}  × 5  =  {Mathf.FloorToInt(time) * 5} pts";
            if (scoreText != null)
                scoreText.text = $"PUNTUACIÓN FINAL:  {score}";
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
