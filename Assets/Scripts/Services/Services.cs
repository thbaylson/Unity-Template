using Template.Achievements;
using Template.Services.Saving;

namespace Template.Services
{
    public static class Services
    {
        public static ISaveService SaveService { get; set; }
        public static IAudioService AudioService { get; set; }
        public static IPauseService PauseService { get; set; }
        public static IAchievementService AchievementService { get; set; }
    }
}
