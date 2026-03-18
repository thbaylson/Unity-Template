using System;
using ServiceLocator = Template.Services.Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting.APIUpdating;

namespace Template
{
    [MovedFrom(true, null, "Assembly-CSharp", "GoldCollector")]
    public class GoldCollector : MonoBehaviour
    {
        public static event Action<int> OnGoldChanged;
        public static event Action<int> OnStateRefresh;

        public static event Action<int, int> OnGoldCollected;

        public int Gold { get; private set; }

        private void Start()
        {
            Gold = ServiceLocator.SaveService.GameDataCache.Player.GoldAmount;
            OnStateRefresh?.Invoke(Gold);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void CollectGold(int amount)
        {
            if (amount <= 0) return;

            Gold += amount;

            var playerData = ServiceLocator.SaveService.GameDataCache.Player;
            playerData.GoldAmount = Gold;
            playerData.TotalGoldCollected += amount;

            ServiceLocator.SaveService.MarkGameDirty();

            OnGoldCollected?.Invoke(amount, Gold);
            OnGoldChanged?.Invoke(Gold);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            OnStateRefresh?.Invoke(Gold);
        }
    }
}
