using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntity.Instances.Storage;
using Conduits.Ports;
using Newtonsoft.Json;
using Chunks;

namespace TileEntity.Instances.CompactMachines {
    public class CompactMachineEnergyPortInstance : TileEntityInstance<CompactMachineEnergyPort>, 
    ISerializableTileEntity ,IConduitPortTileEntity, IEnergyConduitInteractable, ICompactMachineInteractable
    {
        private ulong energy;
        private CompactMachineInstance compactMachine;

        public CompactMachineEnergyPortInstance(CompactMachineEnergyPort tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public ConduitPortLayout GetConduitPortLayout()
        {
            return TileEntityObject.Layout;
        }

        public ref ulong GetEnergy(Vector2Int portPosition)
        {
            return ref energy;
        }
        
        /// <summary>
        /// Allows unbounded throughput but has no storage
        /// </summary>
        public ulong InsertEnergy(ulong insertEnergy, Vector2Int portPosition)
        {
            if (this.energy > 0) {
                return 0;
            }   
            this.energy = insertEnergy;
            return insertEnergy;
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(energy);
        }

        public void SyncToCompactMachine(CompactMachineInstance compactMachine)
        {
            this.compactMachine = compactMachine;
            compactMachine.Inventory.addPort(this,ConduitType.Energy);
        }

        public void Unserialize(string data)
        {
            if (data == null) {
                return;
            }
            energy = JsonConvert.DeserializeObject<uint>(data);
        }
    }

}
