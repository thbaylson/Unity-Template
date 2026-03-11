using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementContainerUI : MonoBehaviour
{
    [SerializeField] private AchievementDefinition achievement;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text displayName;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text flavorText;
    [SerializeField] private RectTransform lockedOverlay;

    public void Initialize(AchievementDefinition achievement)
    {
        this.achievement = achievement;
        var progressState = Services.AchievementService.GetProgress(achievement.Id);
        var isUnlocked = progressState != null && progressState.IsUnlocked;

        icon.sprite = achievement.Icon;
        displayName.text = achievement.DisplayName;
        description.text = achievement.Description;
        flavorText.text = achievement.FlavorText;

        lockedOverlay.gameObject.SetActive(!isUnlocked);
    }
}
