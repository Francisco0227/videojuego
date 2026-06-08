using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int maxEnemies = 5;
    [SerializeField] private float spawnRadius = 8f;

    [Header("Leash")]
    [Tooltip("Los enemigos se reteleportan cuando superan 2× el ancho visible de la cámara")]
    [SerializeField] private float leashCheckInterval = 1f;

    private Transform playerTransform;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        StartCoroutine(SpawnLoop());
        StartCoroutine(LeashLoop());
    }

    // ── Spawn ────────────────────────────────────────────────
    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            int currentEnemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None).Length;
            if (currentEnemies < maxEnemies)
                SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null || playerTransform == null) return;

        Vector2 randomOffset = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPosition = playerTransform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }

    // ── Leash ────────────────────────────────────────────────
    IEnumerator LeashLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(leashCheckInterval);
            if (playerTransform != null)
                CheckLeash();
        }
    }

    void CheckLeash()
    {
        // Distancia máxima = 2 × semiancho visible de la cámara
        float halfW = mainCamera.orthographicSize * mainCamera.aspect;
        float halfH = mainCamera.orthographicSize;
        float leashDist = Mathf.Max(halfW, halfH) * 2f;

        // Radio de reteleporte: justo fuera del borde de cámara
        float reSpawnRadius = Mathf.Max(halfW, halfH) + 1f;

        var enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            float dist = Vector2.Distance(enemy.transform.position, playerTransform.position);
            if (dist > leashDist)
            {
                Vector2 offset = Random.insideUnitCircle.normalized * reSpawnRadius;
                enemy.transform.position = playerTransform.position + new Vector3(offset.x, offset.y, 0f);
            }
        }
    }
}
