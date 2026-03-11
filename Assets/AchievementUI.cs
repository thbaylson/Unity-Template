using System.Collections.Generic;
using UnityEngine;

public class AchievementUI : MonoBehaviour
{
    [SerializeField] private RectTransform contentContainer;
    [SerializeField] private GameObject achievementContainerPrefab;

    private List<AchievementContainerUI> _spawnedContainers = new List<AchievementContainerUI>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        var definitions = Services.AchievementService.GetAllDefinitions();
        if (_spawnedContainers.Count < definitions.Count)
        {
            foreach (AchievementDefinition definition in definitions)
            {
                var container = Instantiate(achievementContainerPrefab, contentContainer);

                var ui = container.GetComponent<AchievementContainerUI>();
                ui.Initialize(definition);
                _spawnedContainers.Add(ui);
            }
        }
        else
        {
            for (int i = 0; i < definitions.Count; i++)
            {
                _spawnedContainers[i].Initialize(definitions[i]);
            }
        }
    }
}
