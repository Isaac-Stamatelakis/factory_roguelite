using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TileEntityModule.Instances.Matrix {
    public interface IMatrixTerminalItemSlotClickListener {
        
    }
    public class MatrixTerminalItemSlotUI : MonoBehaviour, IPointerClickHandler, IMatrixTerminalItemSlotClickListener
    {
        private ItemSlot itemSlot;
        private IMatrixTerminalItemClickReciever reciever;
        public void init(ItemSlot itemSlot, IMatrixTerminalItemClickReciever reciever){
            this.itemSlot = itemSlot;
            this.reciever = reciever;
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) {
                reciever.itemLeftClick(this);
            } else if (eventData.button == PointerEventData.InputButton.Right) {
                reciever.itemRightClick(this);
            } else if (eventData.button == PointerEventData.InputButton.Middle) {
                reciever.itemMiddleClick(this);
            }
        }

        public static MatrixTerminalItemSlotUI newInstance(ItemSlot itemSlot, IMatrixTerminalItemClickReciever reciever) {
            MatrixTerminalItemSlotUI itemSlotUI = GlobalHelper.instantiateFromResourcePath("UI/Matrix/Terminal/MatrixTerminalItemUIPanel").GetComponent<MatrixTerminalItemSlotUI>();
            itemSlotUI.init(itemSlot, reciever);
            return itemSlotUI;
        }
    }
}

