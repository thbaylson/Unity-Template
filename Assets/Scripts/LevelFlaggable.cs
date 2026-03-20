using ServiceLocator = Template.Services.Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting.APIUpdating;

namespace Template
{
    public interface ILevelFlaggable
    {
        string SaveId { get; }
        bool GetFlag();
        void ApplyFlag(bool value);
    }

    /// <summary>
    /// Base class for interactables that store a simple boolean flag in the current scene.
    /// Projects inherit and implement CaptureFlag/ApplyFlag.
    /// </summary>
    [MovedFrom(true, null, "Assembly-CSharp", "LevelFlaggable")]
    [RequireComponent(typeof(Saveable))]
    public abstract class LevelFlaggable : MonoBehaviour, ILevelFlaggable
    {
        [SerializeField] private Saveable saveable;

        public string SaveId => saveable != null ? saveable.Id : "";

        protected virtual void Awake()
        {
            saveable = GetComponent<Saveable>();
        }

        protected virtual void Start()
        {
            ServiceLocator.SaveService.Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.SaveService.SetGameDataCacheFlag(SceneManager.GetActiveScene().name, saveable.Id, GetFlag());
        }

        public abstract bool GetFlag();
        public abstract void ApplyFlag(bool value);
    }
}
