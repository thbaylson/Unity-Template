using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
#endif

namespace Template
{
    /// <summary>
    /// Scene-unique stable ID for saving. Prefab assets should keep this blank.
    /// IDs are generated for scene instances and duplicates are auto-fixed in editor.
    /// </summary>
    [MovedFrom(true, null, "Assembly-CSharp", "Saveable")]
    public sealed class Saveable : MonoBehaviour
    {
        [SerializeField] private string _id;
        public string Id => _id;

#if UNITY_EDITOR
        private void Reset() => EnsureIdInEditor();
        private void OnValidate() => EnsureIdInEditor();

        [ContextMenu("Save System/Generate New Id")]
        private void GenerateNewId()
        {
            if (ShouldSkipIdGeneration()) return;
            _id = Guid.NewGuid().ToString("N");
            MarkDirty();
        }

        private void EnsureIdInEditor()
        {
            if (Application.isPlaying) return;
            if (ShouldSkipIdGeneration()) return;

            // If blank, generate.
            if (string.IsNullOrWhiteSpace(_id))
            {
                _id = Guid.NewGuid().ToString("N");
                MarkDirty();
                return;
            }

            // If duplicate, only regenerate for the "non-owner" copies.
            if (IsDuplicateInScene(_id, out int ownerInstanceId) && ownerInstanceId != GetInstanceID())
            {
                _id = Guid.NewGuid().ToString("N");
                MarkDirty();
            }
        }

        private bool ShouldSkipIdGeneration()
        {
            // Skip if this object is part of a prefab asset (would bake the id into the prefab).
            if (PrefabUtility.IsPartOfPrefabAsset(gameObject)) return true;

            // Skip if we are editing prefab contents in Prefab Mode (Prefab Stage).
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null && stage.IsPartOfPrefabContents(gameObject)) return true;

            return false;
        }

        private bool IsDuplicateInScene(string id, out int ownerInstanceId)
        {
            ownerInstanceId = 0;

            var scene = gameObject.scene;
            if (!scene.IsValid()) return false;

            Saveable owner = null;
            int matches = 0;

            var all = FindObjectsByType<Saveable>(FindObjectsSortMode.None);
            foreach (var e in all)
            {
                if (e == null) continue;
                if (e.gameObject.scene != scene) continue;
                if (!string.Equals(e._id, id, StringComparison.Ordinal)) continue;

                matches++;
                if (owner == null || e.GetInstanceID() < owner.GetInstanceID()) owner = e;
            }

            if (matches <= 1 || owner == null) return false;

            ownerInstanceId = owner.GetInstanceID();
            return true;
        }

        private void MarkDirty()
        {
            EditorUtility.SetDirty(this);

            // Mark the scene dirty so the generated ID is saved.
            if (gameObject.scene.IsValid()) EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif
    }
}
