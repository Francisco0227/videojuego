using UnityEngine;

public class ChestManager : MonoBehaviour
{
    [SerializeField] private ChestUI chestUI;

    void Start()
    {
        if (ExperienceManager.Instance != null)
            ExperienceManager.Instance.OnLevelUp += OnLevelUp;
    }

    void OnDestroy()
    {
        if (ExperienceManager.Instance != null)
            ExperienceManager.Instance.OnLevelUp -= OnLevelUp;
    }

    private void OnLevelUp(int level)
    {
        if (level % 5 != 0) return;
        if (chestUI == null) return;

        // Monedas aleatorias escaladas por nivel
        int coins = Random.Range(10 + level * 2, 30 + level * 3);
        PersistentData.AddCoins(coins);
        chestUI.Show(coins);
    }
}
