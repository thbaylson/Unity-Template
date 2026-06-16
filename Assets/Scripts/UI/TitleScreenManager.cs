using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Scripting.APIUpdating;
using Template.BetterInputHandling;
using ServiceLocator = Template.Services.Services;

namespace Template.UI
{
    /// <summary>
    /// Controls title screen button actions and owns UI input focus while the title screen is visible.
    /// </summary>
    [MovedFrom(true, null, "Assembly-CSharp", "TitleScreenManager")]
    public class TitleScreenManager : MonoBehaviour
    {
        [SerializeField] private string firstGameplayScene = "FlatScene";
        [SerializeField] private string gameplayActionMap = "Player";
        [SerializeField] private string titleActionMap = "UI";

        [Header("Container")]
        [SerializeField] private GameObject buttonContainer;

        [Header("Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitGameButton;

        private void Awake()
        {
            // Bind buttons.
            if (newGameButton) newGameButton.onClick.AddListener(OnNewGame);
            if (loadGameButton) loadGameButton.onClick.AddListener(OnLoadGame);
            if (settingsButton) settingsButton.onClick.AddListener(OnSettings);
            if (quitGameButton) quitGameButton.onClick.AddListener(OnQuit);

#if UNITY_WEBGL
            if (quitGameButton) quitGameButton.gameObject.SetActive(false);
#endif
        }

        private void OnEnable()
        {
            HandleFocusReturned();
            StartCoroutine(HandleFocusReturnedAfterSceneSettles());
        }

        private void RefreshButtonVisibility()
        {
            bool hasSave = ServiceLocator.SaveService.LoadGameExists();
            loadGameButton.gameObject.SetActive(hasSave);
        }

        private void EnsureValidSelection()
        {
            // If Load is hidden, make sure UI selection isn't pointing at it.
            if (EventSystem.current == null) return;

            if (newGameButton != null)
            {
                EventSystem.current.SetSelectedGameObject(newGameButton.gameObject);
            }
        }

        private void OnNewGame()
        {
            // TODO: Add confirmation popup if a save already exists.
            ServiceLocator.SaveService.DeleteGame();
            StartGameplayScene();
        }

        private void OnLoadGame()
        {
            ServiceLocator.SaveService.LoadGame();
            StartGameplayScene();
        }

        private void OnSettings()
        {
            UIService.Instance.ShowSettings(onBack: HandleFocusReturned);
            buttonContainer.SetActive(false);
        }

        private void HandleFocusReturned()
        {
            ClaimTitleInput();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            buttonContainer.SetActive(true);

            RefreshButtonVisibility();
            ConfigureButtonNavigation();
            EnsureValidSelection();
        }

        private IEnumerator HandleFocusReturnedAfterSceneSettles()
        {
            yield return null;
            HandleFocusReturned();
        }

        private void ClaimTitleInput()
        {
            SwitchInputMap(titleActionMap);

            var uiModule = EventSystem.current?.GetComponent<InputSystemUIInputModule>();
            if (uiModule == null)
            {
                return;
            }

            var inputActions = BetterInputService.Instance?.Settings?.InputActions;
            if (inputActions != null && uiModule.actionsAsset != inputActions)
            {
                uiModule.actionsAsset = inputActions;
            }

            uiModule.actionsAsset?.FindActionMap(titleActionMap, false)?.Enable();
            EnableUiAction(uiModule.point);
            EnableUiAction(uiModule.leftClick);
            EnableUiAction(uiModule.middleClick);
            EnableUiAction(uiModule.rightClick);
            EnableUiAction(uiModule.scrollWheel);
            EnableUiAction(uiModule.move);
            EnableUiAction(uiModule.submit);
            EnableUiAction(uiModule.cancel);
        }

        private void StartGameplayScene()
        {
            ReleaseTitleInput();
            SwitchInputMap(gameplayActionMap);
            UnityEngine.SceneManagement.SceneManager.LoadScene(firstGameplayScene);
        }

        private void ReleaseTitleInput()
        {
            var uiModule = EventSystem.current?.GetComponent<InputSystemUIInputModule>();
            if (uiModule == null)
            {
                return;
            }

            DisableUiAction(uiModule.point);
            DisableUiAction(uiModule.leftClick);
            DisableUiAction(uiModule.middleClick);
            DisableUiAction(uiModule.rightClick);
            DisableUiAction(uiModule.scrollWheel);
            DisableUiAction(uiModule.cancel);
        }

        private void ConfigureButtonNavigation()
        {
            var visibleButtons = new List<Button>();
            AddVisibleButton(visibleButtons, newGameButton);
            AddVisibleButton(visibleButtons, loadGameButton);
            AddVisibleButton(visibleButtons, settingsButton);
            AddVisibleButton(visibleButtons, quitGameButton);

            for (var index = 0; index < visibleButtons.Count; index++)
            {
                var up = index > 0 ? visibleButtons[index - 1] : null;
                var down = index < visibleButtons.Count - 1 ? visibleButtons[index + 1] : null;
                SetExplicitNavigation(visibleButtons[index], up, down);
            }
        }

        private static void AddVisibleButton(ICollection<Button> buttons, Button button)
        {
            if (button != null && button.gameObject.activeInHierarchy && button.interactable)
            {
                buttons.Add(button);
            }
        }

        private static void SetExplicitNavigation(Selectable selectable, Selectable up, Selectable down)
        {
            if (selectable == null)
            {
                return;
            }

            var navigation = selectable.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnUp = up;
            navigation.selectOnDown = down;
            navigation.selectOnLeft = null;
            navigation.selectOnRight = null;
            selectable.navigation = navigation;
        }

        private static void SwitchInputMap(string actionMapName)
        {
            BetterInputService.Instance?.SwitchCurrentActionMap(actionMapName);
        }

        private static void EnableUiAction(InputActionReference actionReference)
        {
            actionReference?.action?.Enable();
        }

        private static void DisableUiAction(InputActionReference actionReference)
        {
            actionReference?.action?.Disable();
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
