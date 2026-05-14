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
        public Color hitFlashColor = Color.red;
        public Color shatterColor = Color.white;
    }
}
