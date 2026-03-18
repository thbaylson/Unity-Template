using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Template
{
    [MovedFrom(true, "Template.TransitionByName", "Assembly-CSharp", "LevelTransition")]
    // Simple implementation for testing purposes.
    public class LevelTransition : MonoBehaviour
    {
        [SerializeField] private string nextLevelName;
        [SerializeField] private Vector3 nextLevelPosition;

        private void OnTriggerEnter(Collider other)
        {
            PlayerManager.Instance?.SetPlayerPosition(nextLevelPosition);
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextLevelName);
        }
    }
}
