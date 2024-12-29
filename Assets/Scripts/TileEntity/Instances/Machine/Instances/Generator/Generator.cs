using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Newtonsoft.Json;
using Conduits.Ports;
using UnityEngine.Tilemaps;
using Items.Inventory;
using Recipe.Processor;
using RecipeModule;
using TileEntity.Instances.Machine;
using UnityEngine.Serialization;

namespace TileEntity.Instances.Machines
{
    
    [CreateAssetMenu(fileName = "E~New Generator", menuName = "Tile Entity/Machine/Generator")]
    public class Generator : MachineObject, ITieredTileEntity
    {
        public Tier Tier;
        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new GeneratorInstance(this,tilePosition,tileItem,chunk);
        }

        public Tier GetTier()
        {
            return Tier;
        }
    }
}

