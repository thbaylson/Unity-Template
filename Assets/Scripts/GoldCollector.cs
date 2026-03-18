using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoldCollector : MonoBehaviour
{
    public static event Action<int> OnGoldChanged;
    public static event Action<int> OnStateRefresh;

    public static event Action<int, int> OnGoldCollected;

    public int Gold { get; private set; }

    private void Start()
    {
        Gold = Services.SaveService.GameDataCache.Player.GoldAmount;
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

        var playerData = Services.SaveService.GameDataCache.Player;
        playerData.GoldAmount = Gold;
        playerData.TotalGoldCollected += amount;

        Services.SaveService.MarkGameDirty();

        OnGoldCollected?.Invoke(amount, Gold);
        OnGoldChanged?.Invoke(Gold);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        OnStateRefresh?.Invoke(Gold);
    }
}
