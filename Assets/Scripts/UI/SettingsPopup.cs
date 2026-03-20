using System;
using ServiceLocator = Template.Services.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Scripting.APIUpdating;

namespace Template.UI
{
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

        private Action _onBack;

        private bool suppressSliderEvents;

        private void Awake()
        {
            if (closeButton) closeButton.onClick.AddListener(BackPressed);
        }

        private void OnEnable()
        {
            var audio = ServiceLocator.AudioService;
            if (audio == null) return;

            // Subscribe to service events
            audio.MasterVolumeChanged += OnMasterVolumeChanged;
            audio.MusicVolumeChanged += OnMusicVolumeChanged;
            audio.SfxVolumeChanged += OnSfxVolumeChanged;

            // Subscribe to UI changes
            masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);

            // Initialize UI from service (no feedback loop)
            suppressSliderEvents = true;
            SetSlider(masterSlider, audio.MasterVolumeNormalized);
            SetSlider(musicSlider, audio.MusicVolumeNormalized);
            SetSlider(sfxSlider, audio.SfxVolumeNormalized);
            suppressSliderEvents = false;

            // Set default selected for controller navigation
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

            masterSlider.onValueChanged.RemoveListener(OnMasterSliderChanged);
            musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
            sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);
        }

        // Called by PauseScreen / TitleScreen when opening this screen
        public void Open(Action onBack)
        {
            _onBack = onBack;
            gameObject.SetActive(true);
        }

        // Wire this to Back button OnClick
        public void BackPressed()
        {
            gameObject.SetActive(false);
            _onBack?.Invoke();
            _onBack = null;
        }

        private void OnMasterSliderChanged(float v)
        {
            if (suppressSliderEvents) return;
            ServiceLocator.AudioService?.SetMasterVolume(v);
        }

        private void OnMusicSliderChanged(float v)
        {
            if (suppressSliderEvents) return;
            ServiceLocator.AudioService?.SetMusicVolume(v);
        }

        private void OnSfxSliderChanged(float v)
        {
            if (suppressSliderEvents) return;
            ServiceLocator.AudioService?.SetSfxVolume(v);
        }

        // Service sync in case something else changes volume. TODO: Is this needed?
        private void OnMasterVolumeChanged(float v) => SyncFromService(masterSlider, v);
        private void OnMusicVolumeChanged(float v) => SyncFromService(musicSlider, v);
        private void OnSfxVolumeChanged(float v) => SyncFromService(sfxSlider, v);

        private void SyncFromService(Slider slider, float v)
        {
            suppressSliderEvents = true;
            SetSlider(slider, v);
            suppressSliderEvents = false;
        }

        private static void SetSlider(Slider slider, float v)
        {
            slider.SetValueWithoutNotify(v);
        }
    }
}
