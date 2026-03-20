using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting.APIUpdating;

namespace Template.UI
{
    [MovedFrom(true, null, "Assembly-CSharp", "SelectOnPointerEnter")]
    public class SelectOnPointerEnter : MonoBehaviour, IPointerEnterHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (EventSystem.current == null) return;
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}
