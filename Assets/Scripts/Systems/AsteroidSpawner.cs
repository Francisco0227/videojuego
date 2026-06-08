using UnityEngine;
using System.Collections;

public class AsteroidSpawner : MonoBehaviour
{
    [SerializeField] private GameObject asteroidPrefab;
    [SerializeField] private float minSpawnInterval = 3f;
    [SerializeField] private float maxSpawnInterval = 8f;
    [SerializeField] private float asteroidSpeed = 5f;
    [SerializeField] private float asteroidDamage = 25f;
    [SerializeField] private float spawnMargin = 2f;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));
            if (asteroidPrefab != null)
                SpawnAsteroid();
        }
    }

    private void SpawnAsteroid()
    {
        float halfH = mainCamera.orthographicSize + spawnMargin;
        float halfW = halfH * mainCamera.aspect + spawnMargin;
        Vector3 camPos = mainCamera.transform.position;

        Vector2 spawnPos;
        int edge = Random.Range(0, 4);

        switch (edge)
        {
            case 0: spawnPos = new Vector2(camPos.x + Random.Range(-halfW, halfW), camPos.y + halfH);  break;
            case 1: spawnPos = new Vector2(camPos.x + Random.Range(-halfW, halfW), camPos.y - halfH);  break;
            case 2: spawnPos = new Vector2(camPos.x - halfW, camPos.y + Random.Range(-halfH, halfH));  break;
            default: spawnPos = new Vector2(camPos.x + halfW, camPos.y + Random.Range(-halfH, halfH)); break;
        }

        // Target a random point in the inner half of the screen
        Vector2 target = new Vector2(
            camPos.x + Random.Range(-halfW * 0.4f, halfW * 0.4f),
            camPos.y + Random.Range(-halfH * 0.4f, halfH * 0.4f)
        );

        Vector2 direction = (target - spawnPos).normalized;
        float speed = asteroidSpeed * Random.Range(0.8f, 1.3f);

        GameObject obj = Instantiate(asteroidPrefab, new Vector3(spawnPos.x, spawnPos.y, 0f), Quaternion.identity);
        Asteroid asteroid = obj.GetComponent<Asteroid>();
        asteroid?.Initialize(direction, speed, asteroidDamage);
    }
}
