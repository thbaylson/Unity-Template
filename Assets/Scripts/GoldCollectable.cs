using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Template
{
    [MovedFrom(true, null, "Assembly-CSharp", "GoldCollectable")]
    public class GoldCollectable : LevelFlaggable
    {
        [SerializeField] private int amount = 1;
        private bool collected = false;

        public int Amount => Mathf.Max(1, amount);

        void OnTriggerEnter(Collider other)
        {
            var collect = other.GetComponentInChildren<GoldCollector>();
            if (collect != null)
            {
                collect.CollectGold(amount);
                collected = true;
                Despawn();
            }
        }

        void Despawn()
        {
            gameObject.SetActive(false);
        }

        public override bool GetFlag()
        {
            return collected;
        }

        public override void ApplyFlag(bool value)
        {
            collected = value;
            if (collected)
            {
                Destroy(gameObject);
            }
        }
    }
}
