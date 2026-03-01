using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightweight in-game achievements menu that can be opened with F8 or from the pause menu.
/// </summary>
public class AchievementsMenuOverlay : MonoBehaviour
{
    [SerializeField] private KeyCode toggleKey = KeyCode.F8;

    private bool _isVisible;
    private Vector2 _scrollPosition;
    private Action _onClosed;

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (_isVisible)
            {
                Close();
                return;
            }

            Open();
        }

        if (_isVisible && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    public void Open(Action onClosed = null)
    {
        _onClosed = onClosed;
        _isVisible = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Close()
    {
        _isVisible = false;
        _onClosed?.Invoke();
        _onClosed = null;
    }

    private void OnGUI()
    {
        if (!_isVisible) return;

        var achievementService = Services.AchievementService;
        if (achievementService == null)
        {
            GUI.Box(new Rect(20, 20, 460, 120), "Achievements");
            GUI.Label(new Rect(40, 60, 420, 30), "Achievement service is unavailable.");
            return;
        }

        var definitions = achievementService.GetAllDefinitions();

        var panelRect = new Rect(32, 32, 760, 620);
        GUI.Box(panelRect, "Achievements");

        if (GUI.Button(new Rect(panelRect.xMax - 110, panelRect.y + 10, 90, 28), "Close"))
        {
            Close();
            return;
        }

        if (definitions.Count == 0)
        {
            GUI.Label(new Rect(panelRect.x + 16, panelRect.y + 56, panelRect.width - 32, 24), "No achievement definitions found in Resources/Achievements/Definitions.");
            return;
        }

        var contentHeight = definitions.Count * 112f;
        var viewRect = new Rect(panelRect.x + 12, panelRect.y + 50, panelRect.width - 24, panelRect.height - 62);
        var contentRect = new Rect(0, 0, viewRect.width - 28, contentHeight);
        _scrollPosition = GUI.BeginScrollView(viewRect, _scrollPosition, contentRect);

        DrawDefinitions(definitions, contentRect.width);

        GUI.EndScrollView();
    }

    private void DrawDefinitions(IReadOnlyList<AchievementDefinition> definitions, float width)
    {
        for (int definitionIndex = 0; definitionIndex < definitions.Count; definitionIndex++)
        {
            var definition = definitions[definitionIndex];
            var progressState = Services.AchievementService.GetProgress(definition.Id);
            var isUnlocked = progressState != null && progressState.IsUnlocked;

            var cardRect = new Rect(8, 8 + (definitionIndex * 108), width - 16, 100);
            GUI.Box(cardRect, GUIContent.none);

            var displayName = isUnlocked || !definition.HideUntilUnlocked ? definition.DisplayName : "???";
            var description = isUnlocked || !definition.HideUntilUnlocked ? definition.Description : "Hidden achievement";
            var flavorText = isUnlocked || !definition.HideUntilUnlocked ? definition.FlavorText : "Keep exploring to reveal this achievement.";

            DrawIcon(definition.Icon, cardRect, isUnlocked || !definition.HideUntilUnlocked);

            GUI.Label(new Rect(cardRect.x + 90, cardRect.y + 8, cardRect.width - 100, 20), displayName);
            GUI.Label(new Rect(cardRect.x + 90, cardRect.y + 30, cardRect.width - 100, 32), description);
            GUI.Label(new Rect(cardRect.x + 90, cardRect.y + 62, cardRect.width - 100, 20), flavorText);
            GUI.Label(new Rect(cardRect.x + cardRect.width - 130, cardRect.y + 8, 120, 20), isUnlocked ? "Unlocked" : "Locked");
        }
    }

    private static void DrawIcon(Sprite icon, Rect cardRect, bool shouldShowIcon)
    {
        var iconRect = new Rect(cardRect.x + 10, cardRect.y + 10, 72, 72);

        if (!shouldShowIcon || icon == null)
        {
            GUI.Box(iconRect, "?");
            return;
        }

        GUI.DrawTexture(iconRect, icon.texture, ScaleMode.ScaleToFit);
    }
}
