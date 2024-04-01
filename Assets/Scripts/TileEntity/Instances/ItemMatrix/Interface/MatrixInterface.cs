using System.Collections;
using System.Collections.Generic;
using ConduitModule.Ports;
using ConduitModule.Systems;
using UnityEngine;

namespace TileEntityModule.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Controller", menuName = "Tile Entity/Item Matrix/Interface")]
    public class MatrixInterface : TileEntity, IConduitInteractable, ISolidItemConduitInteractable, IFluidConduitInteractable, IMatrixConduitInteractable
    {
        [SerializeField] private ConduitPortLayout layout;
        private ItemMatrixController controller;

        public ItemSlot extractFluidItem(Vector2Int portPosition)
        {
            return null;
        }

        public ItemSlot extractSolidItem(Vector2Int portPosition)
        {
            return null;
        }

        public ConduitPortLayout getConduitPortLayout()
        {
            return layout;
        }

        public void insertFluidItem(ItemSlot itemSlot, Vector2Int portPosition)
        {
            controller.sendItem(itemSlot);
        }

        public void insertSolidItem(ItemSlot itemSlot, Vector2Int portPosition)
        {
            controller.sendItem(itemSlot);
        }

        public void syncToController(ItemMatrixController matrixController)
        {
            
        }

        public void syncToSystem(MatrixConduitSystem matrixConduitSystem)
        {
            matrixConduitSystem.addInterface(this);
        }
    }
}

