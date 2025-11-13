using UnityEngine;

public class Collectable : MonoBehaviour
{
    [SerializeField] private int amount = 1;

    void OnTriggerEnter(Collider other)
    {
        Collector collect = other.GetComponentInChildren<Collector>();
        if(collect != null)
        {
            collect.CollectItem(amount);
            Despawn();
        }
    }

    void Despawn()
    {
        Destroy(gameObject);
    }
}
