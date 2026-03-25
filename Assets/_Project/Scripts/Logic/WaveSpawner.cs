using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner Instance;

    private Queue<AIAttacker.EnemyWaveEntry> spawnQueue = new Queue<AIAttacker.EnemyWaveEntry>();
    private bool waveActive;
    private int totalInWave;
    private int enemiesEliminated;
    private float spawnInterval = 1.0f;

    // Enemy templates (created at runtime by SceneBuilder)
    public Dictionary<string, GameObject> EnemyTemplates = new Dictionary<string, GameObject>();

    void Awake()
    {
        Instance = this;
    }

    public void SpawnWave(List<AIAttacker.EnemyWaveEntry> wave)
    {
        spawnQueue.Clear();
        foreach (var entry in wave)
            spawnQueue.Enqueue(entry);

        totalInWave = wave.Count;
        enemiesEliminated = 0;
        waveActive = true;
        StartCoroutine(SpawnCoroutine());
    }

    IEnumerator SpawnCoroutine()
    {
        while (spawnQueue.Count > 0)
        {
            var entry = spawnQueue.Dequeue();
            SpawnEnemy(entry.enemyType);
            float interval = Random.Range(0.8f, 1.2f);
            yield return new WaitForSeconds(interval);
        }
    }

    void SpawnEnemy(string enemyType)
    {
        if (!EnemyTemplates.ContainsKey(enemyType)) return;
        if (GridManager.Instance == null) return;

        Vector3 spawnPos = GridManager.Instance.GetEntryWorldPos();
        GameObject template = EnemyTemplates[enemyType];
        GameObject enemy = ObjectPooler.Instance.SpawnFromPool(template, spawnPos, Quaternion.identity);

        var movement = enemy.GetComponent<EnemyMovement>();
        if (movement != null)
            movement.ResetEnemy();
    }

    public void EnemyEliminated()
    {
        enemiesEliminated++;
        if (waveActive && enemiesEliminated >= totalInWave && spawnQueue.Count == 0)
        {
            waveActive = false;
            if (GameManager.Instance != null)
                GameManager.Instance.WaveComplete();
        }
    }
}
