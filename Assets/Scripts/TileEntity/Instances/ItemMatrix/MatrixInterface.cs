using System.Collections;
using System.Collections.Generic;
using ConduitModule.Ports;
using UnityEngine;

namespace TileEntityModule.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Controller", menuName = "Tile Entity/Item Matrix/Interface")]
    public class MatrixInterface : TileEntity, IConduitInteractable, ISolidItemConduitInteractable, IFluidConduitInteractable, IMatrixConduitInteractable
    {
        [SerializeField] private ConduitPortLayout layout;

        public ItemSlot extractFluidItem(Vector2Int portPosition)
        {
            throw new System.NotImplementedException();
        }

        public ItemSlot extractSolidItem(Vector2Int portPosition)
        {
            throw new System.NotImplementedException();
        }

        public ConduitPortLayout getConduitPortLayout()
        {
            return layout;
        }

        public void insertFluidItem(ItemSlot itemSlot, Vector2Int portPosition)
        {
            throw new System.NotImplementedException();
        }

        public void insertSolidItem(ItemSlot itemSlot, Vector2Int portPosition)
        {
            throw new System.NotImplementedException();
        }
    }
}

