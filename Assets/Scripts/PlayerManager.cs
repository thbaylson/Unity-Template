using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [SerializeField] private GameObject playerPrefab;
    private GameObject playerContainer;
    
    private CharacterController playerController;
    private Vector3 playerPosition;
    private bool setPosition;

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelLoaded;
    }

    void Start()
    {
        // If the player instance exists, do not create another
        if (playerContainer != null) return;
        
        if (playerPrefab != null)
        {
            playerContainer = Instantiate(playerPrefab);
            playerContainer.name = playerPrefab.name;
            playerController = playerContainer.GetComponentInChildren<CharacterController>();

            DontDestroyOnLoad(playerContainer);
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelLoaded;
    }

    void OnDestroy()
    {
        if (playerContainer != null)
        {
            Destroy(playerContainer);
        }
    }

    public void SetPlayerPosition(Vector3 newPosition)
    {
        playerPosition = newPosition;
        setPosition = true;
    }

    private void OnLevelLoaded(Scene scene, LoadSceneMode mode)
    {
        if (playerContainer != null && setPosition)
        {
            // Setting the position when a CharacterController is involved requires disabling the CC.
            playerController.enabled = false;
            playerController.transform.position = playerPosition;
            playerController.enabled = true;
            
            setPosition = false;
        }
    }
}
