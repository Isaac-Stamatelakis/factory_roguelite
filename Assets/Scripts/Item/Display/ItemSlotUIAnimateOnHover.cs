using Items;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Item.Display
{
    public class ItemSlotUIAnimateOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private ItemSlotUI mItemSlotUI;
        void Start()
        {
            mItemSlotUI = GetComponentInChildren<ItemSlotUI>();
            if (!mItemSlotUI)
            {
                mItemSlotUI = GetComponent<ItemSlotUI>();
            }

            if (!mItemSlotUI)
            {
                enabled = false;
                return;
            }
            mItemSlotUI.Paused = true;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            mItemSlotUI.Paused = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            mItemSlotUI.Paused = true;
            mItemSlotUI.DisplayFirstFrame();
        }
    }
}
