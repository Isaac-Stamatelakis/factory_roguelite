using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Newtonsoft.Json;
using Conduits.Ports;
using UnityEngine.Tilemaps;
using RecipeModule;
using Items.Inventory;
using Recipe.Processor;
using TileEntity.Instances.Machine;
using UnityEngine.Serialization;

namespace TileEntity.Instances.Machines {
    [CreateAssetMenu(fileName = "New Machine", menuName = "Tile Entity/Machine/Passive")]
    public class PassiveProcessor : MachineObject, ITieredTileEntity
    {
        public Tier Tier;
        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new PassiveProcessorInstance(this,tilePosition,tileItem,chunk);
        }

        public Tier GetTier()
        {
            return Tier;
        }
    }

    
}

