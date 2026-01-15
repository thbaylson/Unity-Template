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
        // TODO: Update this when implementing save/load system.
        bool hasSave = false;

        if (loadGameButton != null)
        {
            loadGameButton.gameObject.SetActive(hasSave);
        }
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
        // TODO: Update this when implementing save/load system.
        UnityEngine.SceneManagement.SceneManager.LoadScene(firstGameplayScene);
    }

    private void OnLoadGame()
    {
        // TODO: Update this when implementing save/load system.
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
