using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TileEntityModule.Instances.Matrix {
    public interface IMatrixTerminalItemSlotClickListener {
        public int getIndex();
        public int getDriveIndex();
        public MatrixDrive getMatrixDrive();
        public GameObject getGameObject();
    }
    public class MatrixTerminalItemSlotUI : MonoBehaviour, IPointerClickHandler, IMatrixTerminalItemSlotClickListener
    {
        private MatrixDrive matrixDrive;
        private int index;
        private int driveIndex;
        private IMatrixTerminalItemClickReciever reciever;
        public void init(MatrixDrive matrixDrive, int index, int driveIndex, IMatrixTerminalItemClickReciever reciever){
            this.matrixDrive = matrixDrive;
            this.index = index;
            this.driveIndex = driveIndex;
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

        public static MatrixTerminalItemSlotUI newInstance(MatrixDrive matrixDrive, int index, int driveIndex, IMatrixTerminalItemClickReciever reciever) {
            MatrixTerminalItemSlotUI itemSlotUI = GlobalHelper.instantiateFromResourcePath("UI/Matrix/Terminal/MatrixTerminalItemUIPanel").GetComponent<MatrixTerminalItemSlotUI>();
            itemSlotUI.init(matrixDrive,index,driveIndex,reciever);
            return itemSlotUI;
        }

        public int getIndex()
        {
            return index;
        }

        public int getDriveIndex()
        {
            return driveIndex;
        }

        public MatrixDrive getMatrixDrive()
        {
            return matrixDrive;
        }

        public GameObject getGameObject()
        {
            return gameObject;
        }
    }
}

