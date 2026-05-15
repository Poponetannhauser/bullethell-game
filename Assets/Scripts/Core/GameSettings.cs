namespace BulletHell.Core
{
    public enum GameMode
    {
        Classic,
        Overheat
    }

    // Persists across scenes without needing a GameObject
    public static class GameSettings
    {
        public static GameMode SelectedMode { get; set; } = GameMode.Classic;
    }
}
