using System;
using Template.BetterInputHandling;
using Template.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Scripting.APIUpdating;

namespace Template.Services
{
    public interface IPauseService
    {
        bool IsPaused { get; }
        event Action<bool> PausedChanged;

        void RegisterPlayerInput(PlayerInput playerInput);
        void RegisterMenu(PauseMenuUI menu);

        void Toggle();
        void SetPaused(bool paused);
        public void SetUIFocus(bool isFocused);
    }

    [MovedFrom(true, null, "Assembly-CSharp", "PauseService")]
    public class PauseService : MonoBehaviour, IPauseService
    {
        public bool IsPaused { get; private set; } = false;
        public event Action<bool> PausedChanged;

        [SerializeField] private string gameplayMap = "Player";
        [SerializeField] private string uiMap = "UI";

        private PlayerInput playerInput;
        private PauseMenuUI menuUI;
        private bool acceptsPauseInput = true;

        /// The reason we assign this input action here instead of setting it up in StarterAssetsInputs is because 
        /// EventSystem was consuming the Close input before it ever got to StarterAssetsInputs. Thus, we subscribe 
        /// directly to the UI Input System's cancel action.
        private InputAction _cancelAction;

        private void Awake()
        {
            if (Services.PauseService != null) return;

            Services.PauseService = this;
        }

        private void OnEnable()
        {
            SetUIFocus(true);
        }

        public void RegisterPlayerInput(PlayerInput playerInput)
        {
            this.playerInput = playerInput;
            RegisterPlayerInputWithUiModule();
            BetterInputService.Instance?.RegisterPlayerInput(playerInput);
            ApplyInputMap();
        }

        public void RegisterMenu(PauseMenuUI menu)
        {
            menuUI = menu;
            menuUI.SetVisible(IsPaused);
        }

        public void Toggle()
        {
            if (IsPaused && !acceptsPauseInput)
            {
                return;
            }

            SetPaused(!IsPaused);
        }

        public void SetPaused(bool paused)
        {
            if (IsPaused == paused) return;

            if (!paused)
            {
                acceptsPauseInput = true;
            }

            IsPaused = paused;

            Time.timeScale = paused ? 0f : 1f;
            Cursor.visible = paused;
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;

            ApplyInputMap();
            menuUI?.SetVisible(paused);

            PausedChanged?.Invoke(paused);
        }

        // Subscreens such as settings own focus while pause remains active, so pause/cancel input should not unpause gameplay.
        public void SetUIFocus(bool isFocused)
        {
            acceptsPauseInput = isFocused;

            if (_cancelAction != null)
            {
                _cancelAction.performed -= OnCancelPerformed;
                _cancelAction = null;
            }

            if (isFocused)
            {
                var uiModule = EventSystem.current?.GetComponent<InputSystemUIInputModule>();
                if (uiModule != null && uiModule.cancel != null)
                {
                    _cancelAction = uiModule.cancel.action;
                    _cancelAction.performed += OnCancelPerformed;
                }
            }
        }

        private void OnCancelPerformed(InputAction.CallbackContext ctx)
        {
            if (!acceptsPauseInput)
            {
                return;
            }

            SetPaused(false);
        }

        private void ApplyInputMap()
        {
            if (playerInput == null) return;

            var targetMap = IsPaused ? uiMap : gameplayMap;
            if (BetterInputService.Instance != null)
            {
                BetterInputService.Instance.SwitchCurrentActionMap(targetMap);
                return;
            }

            playerInput.SwitchCurrentActionMap(targetMap);
        }

        private void RegisterPlayerInputWithUiModule()
        {
            if (playerInput == null)
            {
                return;
            }

            var uiModule = EventSystem.current?.GetComponent<InputSystemUIInputModule>();
            if (uiModule == null)
            {
                return;
            }

            if (uiModule.actionsAsset != playerInput.actions)
            {
                uiModule.actionsAsset = playerInput.actions;
            }

            if (playerInput.uiInputModule != uiModule)
            {
                playerInput.uiInputModule = uiModule;
            }
        }

        private void OnDisable()
        {
            if (_cancelAction != null)
            {
                _cancelAction.performed -= OnCancelPerformed;
                _cancelAction = null;
            }

            if (IsPaused) SetPaused(false);
        }
    }
}
