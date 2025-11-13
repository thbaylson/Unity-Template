using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] private TextMeshProUGUI amountText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
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
        amountText.text = $"COINS: {amount:d3}";
    }
}
