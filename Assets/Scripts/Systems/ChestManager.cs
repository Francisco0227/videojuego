using UnityEngine;

public class ChestManager : MonoBehaviour
{
    public static ChestManager Instance { get; private set; }

    [SerializeField] private ChestUI    chestUI;
    [SerializeField] private GameObject chestPickupPrefab;

    private bool _pendingDrop;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

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

        if (level == 10)
        {
            // Drop físico — el primer enemigo que muera lo suelta
            _pendingDrop = true;
            return;
        }

        // Popup instantáneo para otros múltiplos de 5
        if (chestUI == null) return;
        int coins = Random.Range(10 + level * 2, 30 + level * 3);
        PersistentData.AddCoins(coins);
        chestUI.Show(coins);
    }

    // Llamado por EnemyBase.Die() — solo actúa si hay un drop pendiente
    public void TryDropChest(Vector3 position)
    {
        if (!_pendingDrop || chestPickupPrefab == null) return;
        _pendingDrop = false;

        int level = ExperienceManager.Instance != null
            ? ExperienceManager.Instance.GetCurrentLevel() : 10;
        int coins = Random.Range(10 + level * 2, 30 + level * 3);

        var obj    = Instantiate(chestPickupPrefab, position, Quaternion.identity);
        var pickup = obj.GetComponent<ChestPickup>();
        if (pickup != null) pickup.Initialize(coins, chestUI);
    }
}
