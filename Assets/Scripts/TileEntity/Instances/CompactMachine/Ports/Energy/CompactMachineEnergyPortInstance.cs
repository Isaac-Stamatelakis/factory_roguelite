using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule.Instances.Storage;
using Conduits.Ports;
using Newtonsoft.Json;
using Chunks;

namespace TileEntityModule.Instances.CompactMachines {
    public class CompactMachineEnergyPortInstance : TileEntityInstance<CompactMachineEnergyPort>, 
    ISerializableTileEntity ,IConduitInteractable, IEnergyConduitInteractable, ICompactMachineInteractable
    {
        private int energy;
        private CompactMachineInstance compactMachine;

        public CompactMachineEnergyPortInstance(CompactMachineEnergyPort tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public ConduitPortLayout getConduitPortLayout()
        {
            return tileEntity.Layout;
        }

        public ref int getEnergy(Vector2Int portPosition)
        {
            return ref energy;
        }
        
        /// <summary>
        /// Allows unbounded throughput but has no storage
        /// </summary>
        public int insertEnergy(int insertEnergy,Vector2Int portPosition)
        {
            if (this.energy > 0) {
                return 0;
            }   
            this.energy = insertEnergy;
            return insertEnergy;
        }

        public string serialize()
        {
            return JsonConvert.SerializeObject(energy);
        }

        public void syncToCompactMachine(CompactMachineInstance compactMachine)
        {
            this.compactMachine = compactMachine;
            compactMachine.Inventory.addPort(this,ConduitType.Energy);
        }

        public void unserialize(string data)
        {
            if (data == null) {
                return;
            }
            energy = JsonConvert.DeserializeObject<int>(data);
        }
    }

}
