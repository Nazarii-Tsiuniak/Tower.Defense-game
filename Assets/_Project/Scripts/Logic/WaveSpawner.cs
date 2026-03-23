using UnityEngine;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public float timeBetweenWaves = 10f;
    public float timeBetweenEnemies = 0.8f;

    private float countdown;
    private bool spawning = false;

    void Start()
    {
        countdown = 5f;
    }

    void Update()
    {
        if (spawning) return;

        countdown -= Time.deltaTime;
        if (countdown <= 0f)
        {
            StartCoroutine(SpawnWave());
        }
    }

    IEnumerator SpawnWave()
    {
        spawning = true;
        if (GameManager.Instance != null)
            GameManager.Instance.NextWave();

        int waveNumber = GameManager.Instance != null ? GameManager.Instance.currentWave : 1;
        int enemyCount = 3 + waveNumber * 2;

        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(timeBetweenEnemies);
        }

        spawning = false;
        countdown = timeBetweenWaves;
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;
        if (WaypointPath.Points == null || WaypointPath.Points.Length == 0)
        {
            Debug.LogWarning("WaveSpawner: WaypointPath.Points is empty, cannot spawn enemies.");
            return;
        }

        int index = Random.Range(0, enemyPrefabs.Length);
        Vector3 spawnPos = WaypointPath.Points[0].position;

        GameObject enemy = ObjectPooler.Instance.SpawnFromPool(enemyPrefabs[index], spawnPos, Quaternion.identity);

        // Initialize enemy health from data
        var movement = enemy.GetComponent<EnemyMovement>();
        if (movement == null)
            movement = enemy.GetComponentInChildren<EnemyMovement>();
        if (movement != null && movement.data != null)
        {
            var health = enemy.GetComponent<EnemyHealth>();
            if (health == null)
                health = enemy.AddComponent<EnemyHealth>();
            health.Initialize(movement.data.health, movement.data.cost);
        }
    }
}
