using System.Collections.Generic;
using UnityEngine;

public static class AIAttacker
{
    public struct EnemyWaveEntry
    {
        public string enemyType;
        public EnemyWaveEntry(string type) { enemyType = type; }
    }

    public static List<EnemyWaveEntry> FormWave(int budget, int round)
    {
        var wave = new List<EnemyWaveEntry>();
        int remaining = budget;
        int maxEnemies = 50;

        // AI strategy: mix of enemy types with increasing difficulty
        // Higher rounds → more orcs and ghosts
        float goblinWeight = Mathf.Max(0.2f, 0.6f - round * 0.04f);
        float orcWeight = Mathf.Min(0.4f, 0.15f + round * 0.03f);
        float ghostWeight = 1f - goblinWeight - orcWeight;

        while (remaining > 0 && wave.Count < maxEnemies)
        {
            float roll = Random.value;
            string type;
            int cost;

            if (roll < goblinWeight && remaining >= 10)
            {
                type = "Goblin";
                cost = 10;
            }
            else if (roll < goblinWeight + ghostWeight && remaining >= 20)
            {
                type = "Ghost";
                cost = 20;
            }
            else if (remaining >= 25)
            {
                type = "Orc";
                cost = 25;
            }
            else if (remaining >= 20)
            {
                type = "Ghost";
                cost = 20;
            }
            else if (remaining >= 10)
            {
                type = "Goblin";
                cost = 10;
            }
            else
            {
                break;
            }

            remaining -= cost;
            wave.Add(new EnemyWaveEntry(type));
        }

        // Shuffle for variety
        for (int i = wave.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = wave[i];
            wave[i] = wave[j];
            wave[j] = tmp;
        }

        return wave;
    }
}
