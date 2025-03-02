using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntity.Instances.Storage;
using Conduits.Ports;
using Newtonsoft.Json;
using Chunks;
using Conduits;

namespace TileEntity.Instances.CompactMachines {
    public enum CompactMachinePortType
    {
        Input,
        Output
    }
    public abstract class CompactMachinePortObject : TileEntityObject
    {
        public ConduitPortLayout ConduitPortLayout;
        public CompactMachinePortType PortType;
    }
    public abstract class CompactMachinePortInstance<TObject> : TileEntityInstance<TObject>, ICompactMachineConduitPort, IBreakActionTileEntity, IConduitPortTileEntity where TObject : CompactMachinePortObject
    {
        protected CompactMachineInstance compactMachineInstance;
        public CompactMachinePortInstance(TObject tileEntityObject, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntityObject, positionInChunk, tileItem, chunk)
        {
        }

        public void SyncToCompactMachine(CompactMachineInstance compactMachine)
        {
            this.compactMachineInstance = compactMachine;
            compactMachineInstance.AddPort(tileEntityObject.PortType, GetConduitType(), this);
        }

        public abstract ConduitType GetConduitType();
        public CompactMachinePortType GetPortType()
        {
            return tileEntityObject.PortType;
        }

        public void OnBreak()
        {
            compactMachineInstance.RemovePort(tileEntityObject.PortType,GetConduitType());
        }

        public ConduitPortLayout GetConduitPortLayout()
        {
            return tileEntityObject.ConduitPortLayout;
        }
    }
    public class CompactMachineEnergyPortInstance : CompactMachinePortInstance<CompactMachineEnergyPort>, ISerializableTileEntity, IEnergyConduitInteractable
    {
        private ulong energy;
        public CompactMachineEnergyPortInstance(CompactMachineEnergyPort tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
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
        
        public override ConduitType GetConduitType()
        {
            return ConduitType.Energy;
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
