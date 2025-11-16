using TMPro;
using UnityEngine;

public class GoldUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI amountText;

    private void Awake()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AttachToLayer(transform, UiLayer.Hud);
        }
    }

    private void OnEnable()
    {
        GoldCollector.OnGoldCollected += HandleCoinsChanged;
    }

    private void OnDisable()
    {
        GoldCollector.OnGoldCollected -= HandleCoinsChanged;
    }

    private void HandleCoinsChanged(int amount)
    {
        amountText.text = $"COINS: {amount:d2}";
    }
}
