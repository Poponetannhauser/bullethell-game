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
        public float contactDamage = 10f;

        [Header("Visuals")]
        public Color hitFlashColor = Color.red;
        public Color shatterColor = Color.white;

        [Header("Boss Specific (Optional)")]
        public float entrySpeed = 2f;
        public float targetScreenYOffset = 2.5f;
        public float bounceSpeed = 4f;
        public int radialBulletCount = 12;
        public float phase1FireRate = 2.5f;
        public float phase2FireRate = 0.15f;
        public float spiralAngleIncrement = 20f;
        public float phase2SpeedMultiplier = 1.4f;
    }
}
