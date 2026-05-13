using UnityEngine;

namespace BulletHell.Data
{
    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "BulletHell/Weapon Data")]
    public class WeaponDataSO : ScriptableObject
    {
        public float damage;
        public float bulletSpeed;
        public float fireRate;
        public string bulletPoolKey; // Menyambungkan SO dengan PoolManager poolKey
        public float bulletLifeTime;
    }
}
