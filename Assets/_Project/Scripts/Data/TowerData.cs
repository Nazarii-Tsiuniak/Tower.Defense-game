using UnityEngine;

// Цей рядок дозволяє створювати файл даних через меню правої кнопки миші в Unity
[CreateAssetMenu(fileName = "NewTowerData", menuName = "TD/Tower Data")]
public class TowerData : ScriptableObject
{
    public string towerName;     // Назва вежі 
    public int cost;            // Ціна побудови 
    public float damage;        // Шкода 
    public float range;         // Дальність стрільби 
    public float fireRate;      // Швидкість (інтервал між пострілами) 
    public GameObject projectilePrefab; // Префаб снаряда
    public Sprite visual;       // Спрайт вежі (створений ШІ) [cite: 5]
}