using UnityEngine;

public class GoldCollectable : MonoBehaviour
{
    [SerializeField] private int amount = 1;

    void OnTriggerEnter(Collider other)
    {
        var collect = other.GetComponentInChildren<GoldCollector>();
        if(collect != null)
        {
            collect.CollectGold(amount);
            Despawn();
        }
    }

    void Despawn()
    {
        Destroy(gameObject);
    }
}
