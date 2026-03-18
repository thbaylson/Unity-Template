using System;
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

        public event Action<Collider> BeforeTransition;

        public void ConfigureDestination(string sceneName, Vector3 spawnPosition)
        {
            nextLevelName = sceneName;
            nextLevelPosition = spawnPosition;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null) return;
            if (string.IsNullOrWhiteSpace(nextLevelName)) return;
            if (other.GetComponentInChildren<GoldCollector>() == null) return;

            BeforeTransition?.Invoke(other);
            PlayerManager.Instance?.SetPlayerPosition(nextLevelPosition);
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextLevelName);
        }
    }
}
