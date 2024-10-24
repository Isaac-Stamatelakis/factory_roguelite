using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Newtonsoft.Json;
using RecipeModule.Transmutation;
using Conduits.Ports;
using UnityEngine.Tilemaps;
using RecipeModule.Processors;
using RecipeModule;
using Items.Inventory;

namespace TileEntityModule.Instances.Machines
{
    
    [CreateAssetMenu(fileName = "New Machine", menuName = "Tile Entity/Machine/Processing")]
    public class ProcessingMachine : TileEntity
    {
        
        public AggregatedPoweredMachineProcessor Processor;       
        public Tier Tier;
        public ConduitPortLayout ConduitLayout;

        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new ProcessingMachineInstance(this,tilePosition,tileItem,chunk);
        }
    }
}

