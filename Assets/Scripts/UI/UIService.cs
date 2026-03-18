using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Template.UI
{
    [MovedFrom(true, null, "Assembly-CSharp", "UIService")]
    public class UIService : MonoBehaviour
    {
        public static UIService Instance { get; private set; }
        [SerializeField] private RectTransform hudLayer;
        [SerializeField] private RectTransform screenLayer;
        [SerializeField] private RectTransform popupLayer;

        [SerializeField] private SettingsPopup settingsPopup;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public Transform GetLayer(UiLayer layer)
        {
            return layer switch
            {
                UiLayer.Hud => hudLayer,
                UiLayer.Screen => screenLayer,
                UiLayer.Popup => popupLayer,
                _ => null,
            };
        }

        public void AttachToLayer(Transform t, UiLayer layer)
        {
            var parent = GetLayer(layer);
            if (parent == null)
            {
                Debug.LogError($"UIManager: Layer {layer} not found.");
                return;
            }

            t.SetParent(parent, false);
        }

        public void ShowSettings(Action onBack)
        {
            if (settingsPopup != null)
            {
                settingsPopup.Open(onBack);
            }
        }
    }

    [MovedFrom(true, null, "Assembly-CSharp", "UiLayer")]
    public enum UiLayer
    {
        Hud,
        Screen,
        Popup,
    }
}
