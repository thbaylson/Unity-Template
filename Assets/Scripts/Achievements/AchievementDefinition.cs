using UnityEngine;

/// <summary>
/// Defines one achievement entry that can be authored as content.
/// </summary>
[CreateAssetMenu(menuName = "Achievements/Achievement Definition", fileName = "AchievementDefinition")]
public class AchievementDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string id;

    [Header("Display")]
    [SerializeField] private string displayName;
    [SerializeField] [TextArea] private string description;
    [SerializeField] [TextArea] private string flavorText;
    [SerializeField] private Sprite icon;
    [SerializeField] private bool hideUntilUnlocked;
    [Tooltip("Lower values will display higher in the list with 0 being at the top.")]
    [SerializeField] private int displayOrder;

    [Header("Unlock Condition")]
    [SerializeField] private AchievementConditionType conditionType;
    [SerializeField] private int requiredAmount = 1;
    [SerializeField] private string requiredSceneName;

    public string Id => id;
    public string DisplayName => displayName;
    public string Description => description;
    public string FlavorText => flavorText;
    public Sprite Icon => icon;
    public bool HideUntilUnlocked => hideUntilUnlocked;
    public int DisplayOrder => displayOrder;
    public AchievementConditionType ConditionType => conditionType;
    public int RequiredAmount => requiredAmount;
    public string RequiredSceneName => requiredSceneName;
}
