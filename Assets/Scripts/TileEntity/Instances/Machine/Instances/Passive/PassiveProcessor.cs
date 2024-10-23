using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Newtonsoft.Json;
using RecipeModule.Transmutation;
using Conduits.Ports;
using UnityEngine.Tilemaps;
using RecipeModule;
using Items.Inventory;

namespace TileEntityModule.Instances.Machines {
    [CreateAssetMenu(fileName = "New Machine", menuName = "Tile Entity/Machine/Passive")]
    public class PassiveProcessor : TileEntity
    {
        public PassiveRecipeProcessor RecipeProcessor;
        public Tier Tier;
        public GameObject MachineUIPrefab;
        public ConduitPortLayout ConduitLayout;
        public StandardMachineInventoryLayout Layout;

        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new PassiveProcessorInstance(this,tilePosition,tileItem,chunk);
        }
    }

    
}

