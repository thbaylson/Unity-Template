using UnityEngine;

public class LevelTransition : MonoBehaviour
{
    // Extremely naive implementation for testing purposes.
    private void OnTriggerEnter(Collider other)
    {
        int currentLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        int totalLevels = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
        int nextLevel = (currentLevel + 1) % totalLevels;

        // Level index 0 is the title screen, index 1 is the bootstrapper scene.
        nextLevel = nextLevel < 2 ? 2 : nextLevel;

        UnityEngine.SceneManagement.SceneManager.LoadScene(nextLevel);
    }
}
