using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    [SerializeField] private Button returnToTitleButton;

    [Header("Scene Target")]
    [SerializeField] private string titleSceneName = "Title";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Services.PauseService?.RegisterMenu(this);

        GetComponent<UILayerAttachment>()?.AttachToLayer();

        if (resumeButton) resumeButton.onClick.AddListener(Resume);
        if (returnToTitleButton) returnToTitleButton.onClick.AddListener(ReturnToTitle);
        if (saveButton) saveButton.onClick.AddListener(Save);
        if (settingsButton) settingsButton.onClick.AddListener(Settings);
    }

    public void Resume()
    {
        Services.PauseService?.SetPaused(false);
    }

    public void ReturnToTitle()
    {
        Services.PauseService?.SetPaused(false);
        SceneManager.LoadScene(titleSceneName, LoadSceneMode.Single);
    }

    public void Save()
    {
        Services.SaveService?.SaveGame();
        Services.PauseService?.SetPaused(false);
    }

    public void Settings()
    {
        Services.PauseService?.SetUIFocus(false);
        UIService.Instance.ShowSettings(onBack: HandleFocusReturned);
        SetVisible(false);
    }

    public void SetVisible(bool paused)
    {
        if (menuRoot) menuRoot.SetActive(paused);
        if (buttonContainer) buttonContainer.SetActive(paused);

        // Make gamepad/keyboard navigation work immediately when the menu opens.
        if (paused && EventSystem.current != null && resumeButton != null)
        {
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        }
    }

    private void HandleFocusReturned()
    {
        SetVisible(true);
        Services.PauseService?.SetUIFocus(true);
    }
}
