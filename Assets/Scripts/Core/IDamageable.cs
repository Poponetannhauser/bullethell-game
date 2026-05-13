namespace BulletHell.Core
{
    public interface IDamageable
    {
        // Menggunakan float agar sesuai dengan WeaponDataSO yang memakai float damage
        void TakeDamage(float amount);
    }
}
