using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int lives = 20;
    public int gold = 100;
    public int currentWave = 0;

    public event System.Action OnStatsChanged;

    void Awake()
    {
        Instance = this;
    }

    public void LoseLife()
    {
        lives--;
        OnStatsChanged?.Invoke();
        if (lives <= 0)
        {
            Debug.Log("Game Over!");
        }
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            OnStatsChanged?.Invoke();
            return true;
        }
        return false;
    }

    public void EarnGold(int amount)
    {
        gold += amount;
        OnStatsChanged?.Invoke();
    }

    public void NextWave()
    {
        currentWave++;
        OnStatsChanged?.Invoke();
    }
}
