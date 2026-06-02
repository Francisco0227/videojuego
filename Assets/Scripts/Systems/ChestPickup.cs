using UnityEngine;

public class ChestPickup : MonoBehaviour
{
    [SerializeField] private float pickupRadius = 2f;
    [SerializeField] private float attractSpeed = 6f;
    [SerializeField] private float lifetime     = 90f;

    private int         coinsValue;
    private ChestUI     chestUI;
    private Transform   playerTransform;
    private PlayerStats playerStats;
    private bool        isAttracting;

    public void Initialize(int coins, ChestUI ui)
    {
        coinsValue = coins;
        chestUI    = ui;
    }

    void Start()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerStats     = player.GetComponent<PlayerStats>();
        }
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (playerTransform == null) return;

        float range = playerStats != null ? playerStats.PickupRange : pickupRadius;
        if (Vector2.Distance(transform.position, playerTransform.position) <= range)
            isAttracting = true;

        if (isAttracting)
            transform.position = Vector3.MoveTowards(
                transform.position,
                playerTransform.position,
                attractSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (chestUI != null)
        {
            PersistentData.AddCoins(coinsValue);
            chestUI.Show(coinsValue);
        }

        Destroy(gameObject);
    }
}
