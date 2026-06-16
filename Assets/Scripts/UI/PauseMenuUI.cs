using ServiceLocator = Template.Services.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Scripting.APIUpdating;

namespace Template.UI
{
    [MovedFrom(true, null, "Assembly-CSharp", "PauseMenuUI")]
    public class PauseMenuUI : MonoBehaviour
    {
        public static PauseMenuUI Instance { get; private set; }

        [Header("Containers")]
        [SerializeField] private GameObject menuRoot;
        [SerializeField] private GameObject buttonContainer;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button achievementsButton;
        [SerializeField] private Button returnToTitleButton;

        [Header("Subscreens")]
        [SerializeField] private GameObject achievementsSubscreen;

        [Header("Scene Target")]
        [SerializeField] private string titleSceneName = "Title";

        private GameObject achievementsScreenInstance;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ServiceLocator.PauseService?.RegisterMenu(this);

            GetComponent<UILayerAttachment>()?.AttachToLayer();
            DisableContainerRaycast(buttonContainer);

            if (resumeButton) resumeButton.onClick.AddListener(Resume);
            if (returnToTitleButton) returnToTitleButton.onClick.AddListener(ReturnToTitle);
            if (saveButton) saveButton.onClick.AddListener(Save);
            if (settingsButton) settingsButton.onClick.AddListener(Settings);
            if (achievementsButton) achievementsButton.onClick.AddListener(Achievements);
        }

        public void Resume()
        {
            ServiceLocator.PauseService?.SetPaused(false);
        }

        public void ReturnToTitle()
        {
            ServiceLocator.PauseService?.SetPaused(false);
            SceneManager.LoadScene(titleSceneName, LoadSceneMode.Single);
        }

        public void Save()
        {
            ServiceLocator.SaveService?.SaveGame();
            ServiceLocator.PauseService?.SetPaused(false);
        }

        // TODO: Make this a subscreen like the achievements.
        public void Settings()
        {
            ServiceLocator.PauseService?.SetUIFocus(false);
            UIService.Instance.ShowSettings(onBack: HandleFocusReturned);
            SetVisible(false);
        }

        public void Achievements()
        {
            if (achievementsScreenInstance == null)
            {
                achievementsScreenInstance = Instantiate(achievementsSubscreen, transform);
            }

            ServiceLocator.PauseService?.SetUIFocus(false);
            SetVisible(false);
            // TODO: Make ISubscreen interface with Open and Close methods.
            achievementsScreenInstance.GetComponent<AchievementUI>().Open(onClose: HandleFocusReturned);
        }

        public void SetVisible(bool paused)
        {
            if (menuRoot) menuRoot.SetActive(paused);
            if (buttonContainer) buttonContainer.SetActive(paused);

            // Make gamepad/keyboard navigation work immediately when the menu opens.
            if (paused && EventSystem.current != null && resumeButton != null)
            {
                UIModeSwitcher.Instance?.ShowPointer();
                EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
            }
        }

        private void HandleFocusReturned()
        {
            SetVisible(true);
            ServiceLocator.PauseService?.SetUIFocus(true);
        }

        private static void DisableContainerRaycast(GameObject container)
        {
            if (container == null)
            {
                return;
            }

            var graphic = container.GetComponent<Graphic>();
            if (graphic != null)
            {
                graphic.raycastTarget = false;
            }
        }
    }
}
