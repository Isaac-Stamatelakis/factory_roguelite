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

namespace TileEntity.Instances.Machines
{
    
    [CreateAssetMenu(fileName = "New Machine", menuName = "Tile Entity/Machine/Processing")]
    public class ProcessingMachine : MachineObject, ITieredTileEntity
    {
        public Tier Tier;
       
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new ProcessingMachineInstance(this,tilePosition,tileItem,chunk);
        }

        public Tier GetTier()
        {
            return Tier;
        }
    }
}

