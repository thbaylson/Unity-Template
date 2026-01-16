using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [SerializeField] private GameObject playerPrefab;
    private GameObject playerInstance;

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

    void Start()
    {
        // If the player instance exists, do not create another
        if(playerInstance != null) return;

        if (playerPrefab != null)
        {
            playerInstance = Instantiate(playerPrefab);
            playerInstance.name = playerPrefab.name;
            DontDestroyOnLoad(playerInstance);
        }
        else
        {
            // TODO: Error logging
        }
    }

    void OnDestroy()
    {
        if (playerInstance != null)
        {
            Destroy(playerInstance);
        }
    }
}
