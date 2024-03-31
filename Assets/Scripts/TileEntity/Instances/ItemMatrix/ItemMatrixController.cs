using System.Collections;
using System.Collections.Generic;
using ConduitModule.Ports;
using UnityEngine;
using ConduitModule.Systems;
using UnityEngine.Tilemaps;
using ChunkModule;

namespace TileEntityModule.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Controller", menuName = "Tile Entity/Item Matrix/Controller")]
    public class ItemMatrixController : TileEntity, IMatrixConduitInteractable
    {
        private List<MatrixConduitSystem> systems;
        [SerializeField] private ConduitPortLayout layout;
        public ConduitPortLayout getConduitPortLayout()
        {
            return layout;
        }

        public void addSystem(MatrixConduitSystem matrixConduitSystem) {
            systems.Add(matrixConduitSystem);
        }

        

        public override void initalize(Vector2Int tilePosition, TileBase tileBase, IChunk chunk)
        {
            base.initalize(tilePosition, tileBase, chunk);
            this.systems = new List<MatrixConduitSystem>();

        }

        public void sendSolid(ItemSlot itemSlot) {
            
        }

        public void sendFluid(ItemSlot itemSlot) {
            
        }

        
    }
}

