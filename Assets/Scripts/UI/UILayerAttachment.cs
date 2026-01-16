using UnityEngine;

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
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AttachToLayer(transform, layer);
            isAttached = true;
        }
        else
        {
            Debug.LogError($"UILayerAttachment on {name}: cannot attach to layer {layer} because UIManager is not available.");
        }
    }
}
