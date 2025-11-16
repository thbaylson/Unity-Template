using UnityEngine;

public class UILayerAttachment : MonoBehaviour
{
    [SerializeField] private UiLayer layer = UiLayer.Hud;

    private void Awake()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AttachToLayer(transform, layer);
        }
        else
        {
            Debug.LogWarning($"UILayerAttachment on {name}: no UIManager yet; object will stay in its current parent.");
        }
    }
}
