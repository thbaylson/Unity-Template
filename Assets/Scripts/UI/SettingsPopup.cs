using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using ServiceLocator = Template.Services.Services;
using Template.BetterInputHandling;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UI;

namespace Template.UI
{
    /// <summary>
    /// Displays game settings, including audio sliders and BetterInputHandling controls/rebinding options.
    /// </summary>
    [MovedFrom(true, null, "Assembly-CSharp", "SettingsPopup")]
    public class SettingsPopup : MonoBehaviour
    {
        [Header("Sliders")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;

        [Header("Navigation")]
        [SerializeField] private GameObject defaultSelected;

        private readonly List<ControlRow> controlRows = new List<ControlRow>();
        private readonly List<GameObject> audioContentRoots = new List<GameObject>();
        private Action _onBack;
        private bool suppressSliderEvents;
        private bool hasBuiltTabs;
        private bool isRebinding;
        private SettingsTab activeTab = SettingsTab.Audio;
        private RectTransform controlsContainer;
        private Button audioTabButton;
        private Button controlsTabButton;
        private BetterInputGlyphView previousTabGlyph;
        private BetterInputGlyphView nextTabGlyph;
        private GameObject controlsDefaultSelected;
        private Button controlsResetAllButton;
        private static readonly Color GeneratedButtonNormalColor = new Color(1f, 1f, 1f, 0.18f);
        private static readonly Color GeneratedButtonHighlightedColor = new Color(0.8862745f, 0.7254902f, 0f, 1f);
        private static readonly Color GeneratedButtonPressedColor = new Color(0.78431374f, 0.78431374f, 0.78431374f, 1f);
        private static readonly Color GeneratedButtonDisabledColor = new Color(0.78431374f, 0.78431374f, 0.78431374f, 0.5019608f);

        private enum SettingsTab
        {
            Audio,
            Controls,
        }

        private void Awake()
        {
            if (closeButton) closeButton.onClick.AddListener(BackPressed);
        }

        private void OnEnable()
        {
            var audio = ServiceLocator.AudioService;
            if (audio != null)
            {
                audio.MasterVolumeChanged += OnMasterVolumeChanged;
                audio.MusicVolumeChanged += OnMusicVolumeChanged;
                audio.SfxVolumeChanged += OnSfxVolumeChanged;

                if (masterSlider) masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
                if (musicSlider) musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
                if (sfxSlider) sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);

                suppressSliderEvents = true;
                SetSlider(masterSlider, audio.MasterVolumeNormalized);
                SetSlider(musicSlider, audio.MusicVolumeNormalized);
                SetSlider(sfxSlider, audio.SfxVolumeNormalized);
                suppressSliderEvents = false;
            }

            EnsureTabbedSettingsUI();
            SubscribeToBetterInput();
            activeTab = SettingsTab.Audio;
            SelectTab(activeTab);
        }

        private void OnDisable()
        {
            var audio = ServiceLocator.AudioService;
            if (audio != null)
            {
                audio.MasterVolumeChanged -= OnMasterVolumeChanged;
                audio.MusicVolumeChanged -= OnMusicVolumeChanged;
                audio.SfxVolumeChanged -= OnSfxVolumeChanged;
            }

            if (masterSlider) masterSlider.onValueChanged.RemoveListener(OnMasterSliderChanged);
            if (musicSlider) musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
            if (sfxSlider) sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);

            UnsubscribeFromBetterInput();
            isRebinding = false;
        }

        private void Update()
        {
            if (!hasBuiltTabs || isRebinding)
            {
                return;
            }

            if (Keyboard.current != null)
            {
                if (Keyboard.current.qKey.wasPressedThisFrame)
                {
                    BetterInputService.Instance?.NotifyInputDeviceUsed(Keyboard.current);
                    SwitchTab(-1);
                }
                else if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    BetterInputService.Instance?.NotifyInputDeviceUsed(Keyboard.current);
                    SwitchTab(1);
                }
            }

            if (Gamepad.current == null)
            {
                return;
            }

            if (Gamepad.current.leftShoulder.wasPressedThisFrame)
            {
                BetterInputService.Instance?.NotifyInputDeviceUsed(Gamepad.current);
                SwitchTab(-1);
            }
            else if (Gamepad.current.rightShoulder.wasPressedThisFrame)
            {
                BetterInputService.Instance?.NotifyInputDeviceUsed(Gamepad.current);
                SwitchTab(1);
            }
        }

        public void Open(Action onBack)
        {
            _onBack = onBack;
            gameObject.SetActive(true);
        }

        public void BackPressed()
        {
            gameObject.SetActive(false);
            _onBack?.Invoke();
            _onBack = null;
        }

        private void OnMasterSliderChanged(float value)
        {
            if (suppressSliderEvents) return;
            ServiceLocator.AudioService?.SetMasterVolume(value);
        }

        private void OnMusicSliderChanged(float value)
        {
            if (suppressSliderEvents) return;
            ServiceLocator.AudioService?.SetMusicVolume(value);
        }

        private void OnSfxSliderChanged(float value)
        {
            if (suppressSliderEvents) return;
            ServiceLocator.AudioService?.SetSfxVolume(value);
        }

        private void OnMasterVolumeChanged(float value) => SyncFromService(masterSlider, value);
        private void OnMusicVolumeChanged(float value) => SyncFromService(musicSlider, value);
        private void OnSfxVolumeChanged(float value) => SyncFromService(sfxSlider, value);

        private void SyncFromService(Slider slider, float value)
        {
            suppressSliderEvents = true;
            SetSlider(slider, value);
            suppressSliderEvents = false;
        }

        private static void SetSlider(Slider slider, float value)
        {
            if (slider) slider.SetValueWithoutNotify(value);
        }

        private void EnsureTabbedSettingsUI()
        {
            if (hasBuiltTabs)
            {
                return;
            }

            var root = GetPopupPanelRoot();
            if (root == null)
            {
                return;
            }

            CacheAudioContentRoots(root);

            var tabBar = CreateRect("BetterInputTabBar", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -104f), new Vector2(460f, 34f));
            var tabLayout = tabBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            tabLayout.childAlignment = TextAnchor.MiddleCenter;
            tabLayout.spacing = 8f;
            tabLayout.childControlWidth = false;
            tabLayout.childControlHeight = true;

            previousTabGlyph = CreateGlyph("PreviousTabGlyph", tabBar, new Vector2(28f, 28f));
            previousTabGlyph.BindExplicitControls("<Keyboard>/q", "<Gamepad>/leftShoulder");
            audioTabButton = CreateTabButton("Audio", tabBar, () => SelectTab(SettingsTab.Audio));
            controlsTabButton = CreateTabButton("Controls", tabBar, () => SelectTab(SettingsTab.Controls));
            nextTabGlyph = CreateGlyph("NextTabGlyph", tabBar, new Vector2(28f, 28f));
            nextTabGlyph.BindExplicitControls("<Keyboard>/e", "<Gamepad>/rightShoulder");

            controlsContainer = CreateRect("BetterInputControlsPanel", root, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            controlsContainer.offsetMin = new Vector2(56f, 86f);
            controlsContainer.offsetMax = new Vector2(-56f, -132f);

            var controlsLayout = controlsContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            controlsLayout.childAlignment = TextAnchor.UpperCenter;
            controlsLayout.spacing = 5f;
            controlsLayout.padding = new RectOffset(6, 6, 6, 6);
            controlsLayout.childControlWidth = false;
            controlsLayout.childControlHeight = false;

            BuildControlsRows();
            hasBuiltTabs = true;
        }

        private void BuildControlsRows()
        {
            controlRows.Clear();
            controlsDefaultSelected = null;
            controlsResetAllButton = null;

            var header = CreateText("ControlsHeader", controlsContainer, "Controls", 22f, TextAlignmentOptions.Center);
            header.rectTransform.sizeDelta = new Vector2(620f, 30f);

            var service = BetterInputService.Instance;
            var settings = service != null ? service.Settings : null;
            if (settings == null || settings.RemappableActions.Count == 0)
            {
                var unavailable = CreateText("ControlsUnavailable", controlsContainer, "Controls are unavailable.", 16f, TextAlignmentOptions.Center);
                unavailable.rectTransform.sizeDelta = new Vector2(620f, 32f);
                return;
            }

            foreach (var remappable in settings.RemappableActions)
            {
                CreateControlRow(remappable);
            }

            var resetAll = CreateButton("ResetAllBindingsButton", controlsContainer, "Reset All", ResetAllBindings);
            resetAll.GetComponent<RectTransform>().sizeDelta = new Vector2(160f, 30f);
            controlsResetAllButton = resetAll;
            ConfigureControlsNavigation();
            RefreshControlRows();
        }

        private void CreateControlRow(BetterInputRemappableAction remappable)
        {
            var rowRoot = CreateRect($"{remappable.DisplayName}Row", controlsContainer, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(660f, 32f));
            var rowLayout = rowRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.spacing = 8f;
            rowLayout.childControlWidth = false;
            rowLayout.childControlHeight = true;

            var label = CreateText("Label", rowRoot, remappable.DisplayName, 14f, TextAlignmentOptions.MidlineLeft);
            label.rectTransform.sizeDelta = new Vector2(145f, 28f);

            var glyph = CreateGlyph("Glyph", rowRoot, new Vector2(28f, 28f));
            glyph.BindAction(remappable.ActionReference);

            var bindingText = CreateText("BindingText", rowRoot, string.Empty, 14f, TextAlignmentOptions.MidlineLeft);
            bindingText.rectTransform.sizeDelta = new Vector2(230f, 28f);
            bindingText.enableAutoSizing = true;
            bindingText.fontSizeMin = 10f;
            bindingText.fontSizeMax = 14f;
            bindingText.overflowMode = TextOverflowModes.Ellipsis;

            var rebindButton = CreateButton("RebindButton", rowRoot, "Change", null);
            rebindButton.GetComponent<RectTransform>().sizeDelta = new Vector2(82f, 28f);

            var resetButton = CreateButton("ResetButton", rowRoot, "Reset", null);
            resetButton.GetComponent<RectTransform>().sizeDelta = new Vector2(72f, 28f);

            var row = new ControlRow(remappable, glyph, bindingText, rebindButton, resetButton);
            rebindButton.onClick.AddListener(() => StartRebind(row));
            resetButton.onClick.AddListener(() => ResetBinding(row));
            controlRows.Add(row);

            if (controlsDefaultSelected == null)
            {
                controlsDefaultSelected = rebindButton.gameObject;
            }
        }

        private void SelectTab(SettingsTab tab)
        {
            activeTab = tab;
            foreach (var audioContentRoot in audioContentRoots)
            {
                if (audioContentRoot != null)
                {
                    audioContentRoot.SetActive(activeTab == SettingsTab.Audio);
                }
            }

            if (controlsContainer != null)
            {
                controlsContainer.gameObject.SetActive(activeTab == SettingsTab.Controls);
            }

            ConfigureTabNavigation();
            ConfigureCloseButtonNavigation();
            SetTabButtonSelected(audioTabButton, activeTab == SettingsTab.Audio);
            SetTabButtonSelected(controlsTabButton, activeTab == SettingsTab.Controls);
            RefreshControlRows();
            SelectDefaultForActiveTab();
        }

        private void CacheAudioContentRoots(RectTransform root)
        {
            audioContentRoots.Clear();

            var sliderContainer = FindChildByName(root, "SliderContainer");
            if (sliderContainer != null)
            {
                audioContentRoots.Add(sliderContainer.gameObject);
                return;
            }

            if (masterSlider != null && masterSlider.transform.parent != null)
            {
                audioContentRoots.Add(masterSlider.transform.parent.gameObject);
            }
        }

        private RectTransform GetPopupPanelRoot()
        {
            if (closeButton != null && closeButton.transform.parent is RectTransform closeButtonRoot)
            {
                return closeButtonRoot;
            }

            return transform as RectTransform;
        }

        private void SelectDefaultForActiveTab()
        {
            if (EventSystem.current == null)
            {
                return;
            }

            var selected = activeTab == SettingsTab.Controls ? controlsDefaultSelected : defaultSelected;
            if (selected != null && selected.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(selected);
            }
        }

        private void SwitchTab(int direction)
        {
            if (direction == 0)
            {
                return;
            }

            SelectTab(activeTab == SettingsTab.Audio ? SettingsTab.Controls : SettingsTab.Audio);
        }

        private void ConfigureControlsNavigation()
        {
            for (var index = 0; index < controlRows.Count; index++)
            {
                var row = controlRows[index];
                var previousRow = index > 0 ? controlRows[index - 1] : null;
                var nextRow = index < controlRows.Count - 1 ? controlRows[index + 1] : null;

                SetExplicitNavigation(
                    row.RebindButton,
                    previousRow?.RebindButton ?? controlsTabButton,
                    nextRow?.RebindButton ?? controlsResetAllButton,
                    null,
                    row.ResetButton);

                SetExplicitNavigation(
                    row.ResetButton,
                    previousRow?.ResetButton ?? controlsTabButton,
                    nextRow?.ResetButton ?? controlsResetAllButton,
                    row.RebindButton,
                    null);
            }

            if (controlsResetAllButton != null)
            {
                var lastRow = controlRows.Count > 0 ? controlRows[controlRows.Count - 1] : null;
                SetExplicitNavigation(
                    controlsResetAllButton,
                    lastRow?.RebindButton ?? controlsTabButton,
                    closeButton,
                    null,
                    null);
            }
        }

        private void ConfigureTabNavigation()
        {
            var audioDefault = defaultSelected != null ? defaultSelected.GetComponent<Selectable>() : null;
            var controlsDefault = controlsDefaultSelected != null ? controlsDefaultSelected.GetComponent<Selectable>() : null;

            SetExplicitNavigation(audioTabButton, null, audioDefault, null, controlsTabButton);
            SetExplicitNavigation(controlsTabButton, null, controlsDefault, audioTabButton, null);
        }

        private void ConfigureCloseButtonNavigation()
        {
            if (activeTab == SettingsTab.Controls)
            {
                SetExplicitNavigation(closeButton, controlsResetAllButton, null, null, null);
                return;
            }

            SetAutomaticNavigation(closeButton);
        }

        private static void SetExplicitNavigation(
            Selectable selectable,
            Selectable up,
            Selectable down,
            Selectable left,
            Selectable right)
        {
            if (selectable == null)
            {
                return;
            }

            var navigation = selectable.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnUp = up;
            navigation.selectOnDown = down;
            navigation.selectOnLeft = left;
            navigation.selectOnRight = right;
            selectable.navigation = navigation;
        }

        private static void SetAutomaticNavigation(Selectable selectable)
        {
            if (selectable == null)
            {
                return;
            }

            var navigation = selectable.navigation;
            navigation.mode = Navigation.Mode.Automatic;
            selectable.navigation = navigation;
        }

        private void StartRebind(ControlRow row)
        {
            var service = BetterInputService.Instance;
            if (service == null)
            {
                return;
            }

            isRebinding = true;
            SetControlRowsInteractable(false);
            row.BindingText.text = "Listening...";

            service.StartInteractiveRebind(
                row.Action.ActionReference,
                status => row.BindingText.text = status,
                _ =>
                {
                    isRebinding = false;
                    SetControlRowsInteractable(true);
                    RefreshControlRows();
                    RestoreSelectionAfterFrame(row.RebindButton);
                });
        }

        private void ResetBinding(ControlRow row)
        {
            BetterInputService.Instance?.ResetBinding(row.Action.ActionReference);
            RefreshControlRows();
            RestoreSelectionAfterFrame(row.ResetButton);
        }

        private void ResetAllBindings()
        {
            BetterInputService.Instance?.ResetAllBindings();
            RefreshControlRows();
            RestoreSelectionAfterFrame(controlsResetAllButton);
        }

        private void RefreshControlRows()
        {
            var service = BetterInputService.Instance;
            foreach (var row in controlRows)
            {
                row.Glyph.Refresh();
                row.BindingText.text = service != null ? service.GetBindingDisplayName(row.Action.ActionReference) : string.Empty;
            }
        }

        private void SetControlRowsInteractable(bool interactable)
        {
            foreach (var row in controlRows)
            {
                row.RebindButton.interactable = interactable;
                row.ResetButton.interactable = interactable;
            }
        }

        private void RestoreSelectionAfterFrame(Button button)
        {
            if (!isActiveAndEnabled || button == null)
            {
                return;
            }

            StartCoroutine(RestoreSelectionAfterFrameRoutine(button));
        }

        private static IEnumerator RestoreSelectionAfterFrameRoutine(Button button)
        {
            yield return null;

            if (EventSystem.current != null && button != null && button.interactable && button.gameObject.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(button.gameObject);
            }
        }

        private void SubscribeToBetterInput()
        {
            var service = BetterInputService.Instance;
            if (service == null)
            {
                return;
            }

            service.ActiveDeviceChanged += OnBetterInputChanged;
            service.BindingOverridesChanged += OnBetterInputBindingOverridesChanged;
        }

        private void UnsubscribeFromBetterInput()
        {
            var service = BetterInputService.Instance;
            if (service == null)
            {
                return;
            }

            service.ActiveDeviceChanged -= OnBetterInputChanged;
            service.BindingOverridesChanged -= OnBetterInputBindingOverridesChanged;
        }

        private void OnBetterInputChanged(BetterInputDeviceProfile _)
        {
            previousTabGlyph?.Refresh();
            nextTabGlyph?.Refresh();
            RefreshControlRows();
        }

        private void OnBetterInputBindingOverridesChanged()
        {
            RefreshControlRows();
        }

        private static void SetTabButtonSelected(Button button, bool selected)
        {
            if (button == null)
            {
                return;
            }

            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = selected ? GeneratedButtonHighlightedColor : GeneratedButtonNormalColor;
            }
        }

        private static Button CreateTabButton(string label, RectTransform parent, UnityEngine.Events.UnityAction onClick)
        {
            var button = CreateButton($"{label}TabButton", parent, label, onClick);
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(116f, 30f);
            return button;
        }

        private static Transform FindChildByName(Transform parent, string childName)
        {
            if (parent == null || string.IsNullOrWhiteSpace(childName))
            {
                return null;
            }

            for (var index = 0; index < parent.childCount; index++)
            {
                var child = parent.GetChild(index);
                if (child.name == childName)
                {
                    return child;
                }

                var nestedChild = FindChildByName(child, childName);
                if (nestedChild != null)
                {
                    return nestedChild;
                }
            }

            return null;
        }

        private static Button CreateButton(string name, RectTransform parent, string label, UnityEngine.Events.UnityAction onClick)
        {
            var rect = CreateRect(name, parent, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100f, 30f));
            var image = rect.gameObject.AddComponent<Image>();
            image.color = GeneratedButtonNormalColor;

            var button = rect.gameObject.AddComponent<Button>();
            ConfigureGeneratedButtonVisuals(button, image);
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            var text = CreateText("Text", rect, label, 15f, TextAlignmentOptions.Center);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            text.enableAutoSizing = true;
            text.fontSizeMin = 10f;
            text.fontSizeMax = 15f;
            return button;
        }

        private static void ConfigureGeneratedButtonVisuals(Button button, Graphic targetGraphic)
        {
            button.targetGraphic = targetGraphic;

            var colors = button.colors;
            colors.normalColor = GeneratedButtonNormalColor;
            colors.highlightedColor = GeneratedButtonHighlightedColor;
            colors.selectedColor = GeneratedButtonHighlightedColor;
            colors.pressedColor = GeneratedButtonPressedColor;
            colors.disabledColor = GeneratedButtonDisabledColor;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;
        }

        private static BetterInputGlyphView CreateGlyph(string name, RectTransform parent, Vector2 size)
        {
            var glyphRoot = CreateRect(name, parent, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, size);
            var imageObject = CreateRect("Image", glyphRoot, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var image = imageObject.gameObject.AddComponent<Image>();
            image.raycastTarget = false;

            var fallback = CreateText("Fallback", glyphRoot, string.Empty, 13f, TextAlignmentOptions.Center);
            fallback.rectTransform.anchorMin = Vector2.zero;
            fallback.rectTransform.anchorMax = Vector2.one;
            fallback.rectTransform.offsetMin = Vector2.zero;
            fallback.rectTransform.offsetMax = Vector2.zero;

            var glyph = glyphRoot.gameObject.AddComponent<BetterInputGlyphView>();
            glyph.AssignViews(image, fallback);
            return glyph;
        }

        private static TextMeshProUGUI CreateText(string name, RectTransform parent, string text, float fontSize, TextAlignmentOptions alignment)
        {
            var rect = CreateRect(name, parent, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100f, 30f));
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = Color.white;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.raycastTarget = false;
            return label;
        }

        private static RectTransform CreateRect(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return rect;
        }

        private sealed class ControlRow
        {
            public ControlRow(BetterInputRemappableAction action, BetterInputGlyphView glyph, TMP_Text bindingText, Button rebindButton, Button resetButton)
            {
                Action = action;
                Glyph = glyph;
                BindingText = bindingText;
                RebindButton = rebindButton;
                ResetButton = resetButton;
            }

            public BetterInputRemappableAction Action { get; }
            public BetterInputGlyphView Glyph { get; }
            public TMP_Text BindingText { get; }
            public Button RebindButton { get; }
            public Button ResetButton { get; }
        }
    }
}
