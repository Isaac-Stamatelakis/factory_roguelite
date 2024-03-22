using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ItemModule;

namespace UI {
    public class SerializedItemSlotEditItemPanel : MonoBehaviour, IPointerClickHandler
    {

        private SerializedItemSlot serializedItemSlot;
        private SerializedItemSlotEditorUI editorUI;
        private ItemObject itemObject;
        public void init(SerializedItemSlot serializedItemSlot, SerializedItemSlotEditorUI editorUI, ItemObject itemObject) {
            this.serializedItemSlot = serializedItemSlot;
            this.editorUI = editorUI;
            this.itemObject = itemObject;
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) {
                editorUI.selectItem(itemObject);
            }
        }
    }
}

