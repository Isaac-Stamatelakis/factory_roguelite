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
    public class PassiveProcessor : TileEntityObject
    {
        public RecipeProcessor RecipeProcessor;
        public Tier Tier;
        public ConduitPortLayout ConduitLayout;
        [FormerlySerializedAs("Layout")] public MachineLayoutObject layoutObject;

        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new PassiveProcessorInstance(this,tilePosition,tileItem,chunk);
        }
    }

    
}

