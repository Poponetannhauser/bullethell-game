using UnityEngine;

namespace BulletHell.Data
{
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "BulletHell/Enemy Data")]
    public class EnemyDataSO : ScriptableObject
    {
        public float maxHealth;
        public float moveSpeed;
        public float fireRate;
        public string bulletPoolKey;
        public int scoreValue;

        [Header("Visuals")]
        public Color mainColor = Color.white;
        public Color hitFlashColor = Color.red;
    }
}
