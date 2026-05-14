namespace BulletHell.Core
{
    public enum GameMode
    {
        Classic,
        Overheat
    }

    /// <summary>
    /// Kelas statis untuk menyimpan pengaturan yang harus bertahan antar scene.
    /// Tidak memerlukan GameObject — cukup diakses langsung: GameSettings.SelectedMode
    /// </summary>
    public static class GameSettings
    {
        public static GameMode SelectedMode { get; set; } = GameMode.Classic;
    }
}
