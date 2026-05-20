using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 3f;   // Segundos entre spawns
    [SerializeField] private int maxEnemies = 5;   // Máximo en escena
    [SerializeField] private float spawnRadius = 8f;   // Distancia del jugador

    private Transform playerTransform;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // Contar cuántos enemigos hay en escena
            // FindObjectsByType es la versión moderna de FindObjectsOfType
            int currentEnemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None).Length;

            if (currentEnemies < maxEnemies)
                SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null || playerTransform == null) return;

        // Generar posición aleatoria en un círculo alrededor del jugador
        // Random.insideUnitCircle da un punto aleatorio dentro de un círculo de radio 1
        // Lo multiplicamos por el radio que queremos
        Vector2 randomOffset = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPosition = playerTransform.position + new Vector3(randomOffset.x, randomOffset.y, 0);

        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }
}