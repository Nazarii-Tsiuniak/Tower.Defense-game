using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "TD/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public int health;
    public float speed;
    public int cost;
    public int rewardGold;
    public bool immuneToSlow;
    public Sprite visual;
}