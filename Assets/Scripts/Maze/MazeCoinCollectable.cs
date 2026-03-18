using ServiceLocator = Template.Services.Services;
using UnityEngine;

namespace Template.Maze
{
    public class MazeCoinCollectable : MonoBehaviour
    {
        [SerializeField] private string coinId;
        [SerializeField] private int amount = 1;

        public void Initialize(string id, int goldAmount)
        {
            coinId = id;
            amount = Mathf.Max(1, goldAmount);
        }

        private void OnTriggerEnter(Collider other)
        {
            var collector = other.GetComponentInChildren<GoldCollector>();
            if (collector == null) return;

            collector.CollectGold(amount);
            ServiceLocator.MazeSaveService?.MarkCoinCollected(coinId);

            Destroy(gameObject);
        }
    }
}
