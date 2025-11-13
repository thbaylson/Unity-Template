using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] private TextMeshProUGUI amountText;

    void Awake()
    {
        if(Instance == null || Instance.enabled == false)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }        
    }

    public void UpdateUI(int amount)
    {
        amountText.text = $"COINS: {amount:d2}";
    }
}
