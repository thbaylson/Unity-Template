using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementContainerUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Sprite lockedIcon;
    [SerializeField] private TMP_Text displayName;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text flavorText;
    [SerializeField] private RectTransform lockedOverlay;

    public void Initialize(AchievementDefinition achievement)
    {
        var achievementService = Services.AchievementService;
        var progressState = achievementService.GetProgress(achievement.Id);
        var isUnlocked = progressState != null && progressState.IsUnlocked;
        var displayProgress = achievementService.GetDisplayProgress(achievement);

        icon.sprite = isUnlocked ? achievement.Icon : lockedIcon;
        displayName.text = isUnlocked ? achievement.DisplayName : "???";
        description.text = AchievementProgressTextFormatter.BuildDescriptionText(
            achievement.Description,
            displayProgress);
        flavorText.text = isUnlocked ? achievement.FlavorText : "";

        //lockedOverlay.gameObject.SetActive(!isUnlocked);
    }
}
