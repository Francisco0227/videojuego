using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private int   enemiesKilled = 0;
    private float timeAlive     = 0f;
    private bool  tracking      = false;

    public event Action<int> OnEnemyKilled;

    public int   EnemiesKilled => enemiesKilled;
    public float TimeAlive     => timeAlive;
    public int   Score         => enemiesKilled * 100 + Mathf.FloorToInt(timeAlive) * 5;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()  { tracking = true; }

    void Update()
    {
        if (tracking) timeAlive += Time.deltaTime;
    }

    public void RegisterKill()
    {
        if (!tracking) return;
        enemiesKilled++;
        OnEnemyKilled?.Invoke(enemiesKilled);
    }

    public void StopTracking() { tracking = false; }
}
