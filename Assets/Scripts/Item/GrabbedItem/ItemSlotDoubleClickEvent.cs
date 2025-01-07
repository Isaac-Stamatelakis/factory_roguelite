using Item.Display.ClickHandlers;
using Item.Slot;
using UnityEngine;

namespace Item.GrabbedItem
{
    public class ItemSlotDoubleClickEvent
    {
        public ItemSlotUIClickHandler ClickHandler;
        private ItemSlot doubleClickSlot;
        private static readonly float lifeTime = 0.33f;
        private float time;
        public ItemSlotDoubleClickEvent(ItemSlotUIClickHandler clickHandler)
        {
            this.ClickHandler = clickHandler;
        }

        public void Tick()
        {
            time += Time.deltaTime;
        }
        

        public bool Expired()
        {
            return time > lifeTime;
        }
    }
}