using UnityEngine;

public enum AttackType { Single, AoE, Slow }

[CreateAssetMenu(fileName = "NewTowerData", menuName = "TD/Tower Data")]
public class TowerData : ScriptableObject
{
    public string towerName;
    public int cost;
    public float damage;
    public float range;
    public float fireRate;
    public AttackType attackType;
    public float slowAmount;
    public float slowDuration;
    public float aoeRadius;
    public Sprite visual;
    public GameObject projectilePrefab;
}