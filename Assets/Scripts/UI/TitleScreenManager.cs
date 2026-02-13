using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TitleScreenManager : MonoBehaviour
{
    [SerializeField] private string firstGameplayScene = "FlatScene";

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
    }

    private void OnEnable()
    {
        HandleFocusReturned();
    }

    private void RefreshButtonVisibility()
    {
        bool hasSave = Services.SaveService.LoadGameExists();
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
        Services.SaveService.DeleteGame();
        UnityEngine.SceneManagement.SceneManager.LoadScene(firstGameplayScene);
    }

    private void OnLoadGame()
    {
        Services.SaveService.LoadGame();
        UnityEngine.SceneManagement.SceneManager.LoadScene(firstGameplayScene);
    }

    private void OnSettings()
    {
        UIService.Instance.ShowSettings(onBack: HandleFocusReturned);
        buttonContainer.SetActive(false);
    }

    private void HandleFocusReturned()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        buttonContainer.SetActive(true);

        RefreshButtonVisibility();
        EnsureValidSelection();
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
