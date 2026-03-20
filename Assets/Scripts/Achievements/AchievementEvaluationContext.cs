using System.Collections.Generic;
using System.Linq;

namespace Template.Achievements
{
    public readonly struct AchievementEvaluationContext
    {
        public AchievementEvaluationContext(
            int currentGoldOwned,
            int totalGoldCollected,
            int totalEmotesPerformed,
            IReadOnlyCollection<string> visitedSceneNames,
            string mostRecentScene)
            : this(
                currentGoldOwned,
                totalGoldCollected,
                totalEmotesPerformed,
                0,
                visitedSceneNames,
                mostRecentScene)
        {
        }

        public AchievementEvaluationContext(
            int currentGoldOwned,
            int totalGoldCollected,
            int totalEmotesPerformed,
            int totalMazesSolved,
            IReadOnlyCollection<string> visitedSceneNames,
            string mostRecentScene)
        {
            CurrentGoldOwned = currentGoldOwned;
            TotalGoldCollected = totalGoldCollected;
            TotalEmotesPerformed = totalEmotesPerformed;
            TotalMazesSolved = totalMazesSolved;
            VisitedSceneNames = visitedSceneNames;
            MostRecentScene = mostRecentScene;
        }

        public int CurrentGoldOwned { get; }
        public int TotalGoldCollected { get; }
        public int TotalEmotesPerformed { get; }
        public int TotalMazesSolved { get; }
        public IReadOnlyCollection<string> VisitedSceneNames { get; }
        public string MostRecentScene { get; }

        public bool HasVisitedScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName) || VisitedSceneNames == null) return false;

            return VisitedSceneNames.Contains(sceneName);
        }
    }
}
