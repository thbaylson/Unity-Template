using TMPro;
using UnityEngine;

public class GoldUI : MonoBehaviour
{
    public static GoldUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI amountText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        GoldCollector.OnGoldChanged += HandleCoinsChanged;
        GoldCollector.OnStateRefresh += HandleCoinsChanged;
    }

    private void OnDisable()
    {
        GoldCollector.OnGoldChanged -= HandleCoinsChanged;
        GoldCollector.OnStateRefresh -= HandleCoinsChanged;
    }

    private void HandleCoinsChanged(int amount)
    {
        amountText.text = $"COINS: {amount:d2}";
    }
}
