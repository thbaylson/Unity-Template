using System;
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
            SelectTab(activeTab);

            if (defaultSelected != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(defaultSelected);
            }
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

            var root = transform as RectTransform;
            if (root == null)
            {
                return;
            }

            CacheAudioContentRoots(root);

            var tabBar = CreateRect("BetterInputTabBar", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(420f, 36f));
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
            controlsContainer.offsetMin = new Vector2(48f, 72f);
            controlsContainer.offsetMax = new Vector2(-48f, -96f);

            var controlsLayout = controlsContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            controlsLayout.childAlignment = TextAnchor.UpperCenter;
            controlsLayout.spacing = 8f;
            controlsLayout.padding = new RectOffset(8, 8, 8, 8);
            controlsLayout.childControlWidth = true;
            controlsLayout.childControlHeight = false;

            BuildControlsRows();
            hasBuiltTabs = true;
        }

        private void BuildControlsRows()
        {
            controlRows.Clear();

            var header = CreateText("ControlsHeader", controlsContainer, "Controls", 22f, TextAlignmentOptions.Center);
            header.rectTransform.sizeDelta = new Vector2(480f, 34f);

            var service = BetterInputService.Instance;
            var settings = service != null ? service.Settings : null;
            if (settings == null || settings.RemappableActions.Count == 0)
            {
                var unavailable = CreateText("ControlsUnavailable", controlsContainer, "Controls are unavailable.", 16f, TextAlignmentOptions.Center);
                unavailable.rectTransform.sizeDelta = new Vector2(480f, 32f);
                return;
            }

            foreach (var remappable in settings.RemappableActions)
            {
                CreateControlRow(remappable);
            }

            var resetAll = CreateButton("ResetAllBindingsButton", controlsContainer, "Reset All", ResetAllBindings);
            resetAll.GetComponent<RectTransform>().sizeDelta = new Vector2(180f, 34f);
            RefreshControlRows();
        }

        private void CreateControlRow(BetterInputRemappableAction remappable)
        {
            var rowRoot = CreateRect($"{remappable.DisplayName}Row", controlsContainer, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500f, 38f));
            var rowLayout = rowRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.spacing = 10f;
            rowLayout.childControlWidth = false;
            rowLayout.childControlHeight = true;

            var label = CreateText("Label", rowRoot, remappable.DisplayName, 16f, TextAlignmentOptions.MidlineLeft);
            label.rectTransform.sizeDelta = new Vector2(150f, 30f);

            var glyph = CreateGlyph("Glyph", rowRoot, new Vector2(30f, 30f));
            glyph.BindAction(remappable.ActionReference);

            var bindingText = CreateText("BindingText", rowRoot, string.Empty, 15f, TextAlignmentOptions.MidlineLeft);
            bindingText.rectTransform.sizeDelta = new Vector2(110f, 30f);

            var rebindButton = CreateButton("RebindButton", rowRoot, "Change", null);
            rebindButton.GetComponent<RectTransform>().sizeDelta = new Vector2(90f, 30f);

            var resetButton = CreateButton("ResetButton", rowRoot, "Reset", null);
            resetButton.GetComponent<RectTransform>().sizeDelta = new Vector2(78f, 30f);

            var row = new ControlRow(remappable, glyph, bindingText, rebindButton, resetButton);
            rebindButton.onClick.AddListener(() => StartRebind(row));
            resetButton.onClick.AddListener(() => ResetBinding(row));
            controlRows.Add(row);
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

            SetTabButtonSelected(audioTabButton, activeTab == SettingsTab.Audio);
            SetTabButtonSelected(controlsTabButton, activeTab == SettingsTab.Controls);
            RefreshControlRows();
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

        private void SwitchTab(int direction)
        {
            if (direction == 0)
            {
                return;
            }

            SelectTab(activeTab == SettingsTab.Audio ? SettingsTab.Controls : SettingsTab.Audio);
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
                });
        }

        private void ResetBinding(ControlRow row)
        {
            BetterInputService.Instance?.ResetBinding(row.Action.ActionReference);
            RefreshControlRows();
        }

        private void ResetAllBindings()
        {
            BetterInputService.Instance?.ResetAllBindings();
            RefreshControlRows();
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
                image.color = selected ? new Color(1f, 0.82f, 0.12f, 0.92f) : new Color(1f, 1f, 1f, 0.18f);
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
            image.color = new Color(1f, 1f, 1f, 0.18f);

            var button = rect.gameObject.AddComponent<Button>();
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            var text = CreateText("Text", rect, label, 15f, TextAlignmentOptions.Center);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            return button;
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
