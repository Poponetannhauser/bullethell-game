namespace BulletHell.Core
{
    public enum GameMode
    {
        Classic,
        Overheat
    }

    // Menyimpan pilihan game mode
    public static class GameSettings
    {
        public static GameMode SelectedMode { get; set; } = GameMode.Classic;
    }
}
