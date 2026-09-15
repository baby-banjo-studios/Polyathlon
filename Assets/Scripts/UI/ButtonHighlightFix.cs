using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BabyBanjo.Core.UI
{
    [RequireComponent(typeof(Selectable))]
    public class ButtonHighlightFix : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDeselectHandler
    {
        public void OnPointerEnter(PointerEventData _)
        {
            if (!EventSystem.current.alreadySelecting)
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (EventSystem.current.currentSelectedGameObject == gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            GetComponent<Selectable>().OnPointerExit(null);
        }
    }
}