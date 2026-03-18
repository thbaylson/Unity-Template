using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AchievementUI : MonoBehaviour
{
    [SerializeField] private RectTransform contentContainer;
    [SerializeField] private GameObject achievementContainerPrefab;
    [SerializeField] private Button closeButton;

    private Action _onClose;

    private List<AchievementContainerUI> _spawnedContainers = new List<AchievementContainerUI>();
    private IReadOnlyList<AchievementDefinition> _achievementDefinitions;

    private void Awake()
    {
        _achievementDefinitions = Services.AchievementService.GetAllDefinitions();
        if (closeButton) closeButton.onClick.AddListener(Close);
    }

    public void Open(Action onClose)
    {
        _onClose = onClose;
        RefreshUI();
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
        _onClose?.Invoke();
        _onClose = null;
    }

    // TODO: Connect this to AchievementService's AchievementsChanged event so we don't have to rebuild every time this screen is opened.
    private void RefreshUI()
    {
        var showableDefinitions = _achievementDefinitions
            .Where(def => Services.AchievementService.GetProgress(def.Id).IsUnlocked || !def.HideUntilUnlocked).ToList();

        // Order by unlock
        showableDefinitions = showableDefinitions.OrderByDescending(def => Services.AchievementService.GetProgress(def.Id).IsUnlocked).ToList();

        foreach(AchievementContainerUI container in _spawnedContainers)
        {
            Destroy(container.gameObject);
        }
        _spawnedContainers.Clear();

        foreach (AchievementDefinition definition in showableDefinitions)
        {
            var container = Instantiate(achievementContainerPrefab, contentContainer);

            var ui = container.GetComponent<AchievementContainerUI>();
            ui.Initialize(definition);
            _spawnedContainers.Add(ui);
        }
    }
}
