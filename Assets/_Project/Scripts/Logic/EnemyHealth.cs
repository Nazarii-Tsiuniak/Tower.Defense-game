using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private int maxHealth;
    private int currentHealth;
    private int reward;

    public void Initialize(int health, int goldReward)
    {
        maxHealth = health;
        currentHealth = health;
        reward = goldReward;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= Mathf.RoundToInt(damage);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    public float GetHealthRatio()
    {
        return (float)currentHealth / maxHealth;
    }

    void Die()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.EarnGold(reward);
        ObjectPooler.Instance.ReturnToPool(transform.root.gameObject);
    }
}
