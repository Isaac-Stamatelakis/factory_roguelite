using System;
using Item.Slot;
using Items;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TileEntity.Instances.CompactMachine.UI.Selector
{
    public class CompactMachineHashSelectorElement : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI mNameText;
        [SerializeField] private TextMeshProUGUI mHashText;
        [SerializeField] private ItemSlotUI mItemSlot;
        private Action<string> onSelect;
        private HashSelectorDisplayData displayData;
        internal HashSelectorDisplayData DisplayData => displayData;
        internal void Display(HashSelectorDisplayData displayData)
        {
            this.displayData = displayData;
            mNameText.text = displayData.MetaData.Name;
            mHashText.text = displayData.Hash;
            ItemObject itemObject = ItemRegistry.GetInstance().GetItemObject(displayData.MetaData.TileID);
            ItemSlot itemSlot = new ItemSlot(itemObject,1,null);
            mItemSlot.Display(itemSlot);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button is PointerEventData.InputButton.Left or PointerEventData.InputButton.Right)
            {
                displayData.OnHashSelect?.Invoke(displayData.Hash);
                CanvasController.Instance.PopStack();
            }
        }
    }
}
