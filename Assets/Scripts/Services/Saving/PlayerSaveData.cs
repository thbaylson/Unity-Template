using System;
using System.Collections.Generic;
using Template.Achievements;

namespace Template.Services.Saving
{
    [Serializable]
    public class PlayerSaveData
    {
        public int GoldAmount = 0;
        public int TotalGoldCollected = 0;
        public int TotalEmotesPerformed = 0;
        public int TotalMazesSolved = 0;
        public List<string> VisitedSceneNames = new List<string>();
        public List<AchievementProgressState> Achievements = new List<AchievementProgressState>();
    }
}
