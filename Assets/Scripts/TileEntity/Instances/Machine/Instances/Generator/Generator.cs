using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Newtonsoft.Json;
using RecipeModule.Transmutation;
using Conduits.Ports;
using UnityEngine.Tilemaps;
using Items.Inventory;
using RecipeModule;

namespace TileEntityModule.Instances.Machines
{
    
    [CreateAssetMenu(fileName = "E~New Generator", menuName = "Tile Entity/Machine/Generator")]
    public class Generator : TileEntity
    {
        public EnergyRecipeProcessor EnergyRecipeProcessor;
        public Tier Tier;
        public ConduitPortLayout Layout;

        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new GeneratorInstance(this,tilePosition,tileItem,chunk);
        }
    }
}

