using UnityEngine;

namespace BulletHell.Data
{
    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "BulletHell/Weapon Data")]
    public class WeaponDataSO : ScriptableObject
    {
        public float damage;
        public float bulletSpeed;
        public float fireRate;
        public string bulletPoolKey;
        public float bulletLifeTime;

        [Header("Overheat Stats (Mode Overheat Only)")]
        public float heatPerShot;
        public float coolDownRate;
        public float overheatCooldownDuration;
    }
}
