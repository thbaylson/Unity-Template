using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Template.UI
{
    [MovedFrom(true, null, "Assembly-CSharp", "UILayerAttachment")]
    public class UILayerAttachment : MonoBehaviour
    {
        [SerializeField] private UiLayer layer = UiLayer.Hud;
        private bool isAttached = false;

        private void Awake()
        {
            if (!isAttached)
            {
                AttachToLayer();
            }
        }

        public void AttachToLayer()
        {
            if (UIService.Instance != null)
            {
                UIService.Instance.AttachToLayer(transform, layer);
                isAttached = true;
            }
            else
            {
                Debug.LogError($"UILayerAttachment on {name}: cannot attach to layer {layer} because UIManager is not available.");
            }
        }
    }
}
