namespace Template.Achievements
{
    /// <summary>
    /// Defines the canonical signal keys used by the achievement system to identify
    /// progression events that may trigger achievement evaluation.
    /// </summary>
    public static class AchievementSignalKeys
    {
        public const string GoldCollected = "progress.gold.collected";
        public const string GoldOwnedChanged = "progress.gold.owned.changed";
        public const string EmotePerformed = "progress.emote.performed";
        public const string SceneVisited = "progress.scene.visited";
    }
}
