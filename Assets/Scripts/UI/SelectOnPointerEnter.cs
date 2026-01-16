using UnityEngine;
using UnityEngine.EventSystems;


public class SelectOnPointerEnter : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}
