using System.Collections;
using System.Collections.Generic;
using ConduitModule.Ports;
using ConduitModule.Systems;
using UnityEngine;

namespace TileEntityModule.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Controller", menuName = "Tile Entity/Item Matrix/Recipe Encoder")]
    public class MatrixRecipeEncoder : TileEntity, IRightClickableTileEntity, IMatrixConduitInteractable
    {
        [SerializeField] private ConduitPortLayout layout;
        public ConduitPortLayout getConduitPortLayout()
        {
            return layout;
        }

        public void onRightClick()
        {
            
        }

        public void syncToController(ItemMatrixController matrixController)
        {
            
        }

        public void syncToSystem(MatrixConduitSystem matrixConduitSystem)
        {
            
        }
    }

}
