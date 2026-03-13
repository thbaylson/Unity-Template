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
        var progressState = Services.AchievementService.GetProgress(achievement.Id);
        var isUnlocked = progressState != null && progressState.IsUnlocked;

        icon.sprite = progressState.IsUnlocked ? achievement.Icon : lockedIcon;
        displayName.text = progressState.IsUnlocked ? achievement.DisplayName : "???";
        description.text = achievement.Description;
        flavorText.text = progressState.IsUnlocked ? achievement.FlavorText : "";

        //lockedOverlay.gameObject.SetActive(!isUnlocked);
    }
}
