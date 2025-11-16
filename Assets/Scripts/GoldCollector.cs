using System;
using UnityEngine;

public class GoldCollector : MonoBehaviour
{
    public static event Action<int> OnGoldCollected;

    public int Gold { get; private set; }

    public void CollectGold(int amount)
    {
        Gold += amount;
        OnGoldCollected?.Invoke(Gold);
    }
}
