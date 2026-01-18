using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TitleScreenManager : MonoBehaviour
{
    [SerializeField] private string firstGameplayScene = "SmallScene";

    [Header("Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button quitGameButton;

    private void Awake()
    {
        // Bind buttons.
        if (newGameButton) newGameButton.onClick.AddListener(OnNewGame);
        if (loadGameButton) loadGameButton.onClick.AddListener(OnLoadGame);
        if (quitGameButton) quitGameButton.onClick.AddListener(OnQuit);
    }

    private void OnEnable()
    {
        RefreshButtonVisibility();
        EnsureValidSelection();
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

    private void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
