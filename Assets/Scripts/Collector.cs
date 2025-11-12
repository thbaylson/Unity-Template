using UnityEngine;

public class Collector : MonoBehaviour
{
    private int collectedItemCount = 0;

    void Start()
    {
        UIManager.Instance?.UpdateUI(collectedItemCount);
    }

    // This might need an object ref later
    public void CollectItem(int itemCount)
    {
        collectedItemCount += itemCount;
        UIManager.Instance?.UpdateUI(collectedItemCount);
    }
}
