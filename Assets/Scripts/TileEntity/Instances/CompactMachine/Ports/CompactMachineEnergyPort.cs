using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule.Instances.Storage;
using Conduits.Ports;
using Newtonsoft.Json;

namespace TileEntityModule.Instances.CompactMachines {
    [CreateAssetMenu(fileName = "E~New Energy Port", menuName = "Tile Entity/Compact Machine/Port/Energy")]
    public class CompactMachineEnergyPort : TileEntity, ISerializableTileEntity ,IConduitInteractable, IEnergyConduitInteractable, ICompactMachineInteractable
    {
        [SerializeField] public ConduitPortLayout conduitPortLayout;
        private int energy;
        private CompactMachine compactMachine;

        public ConduitPortLayout getConduitPortLayout()
        {
            return conduitPortLayout;
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

        public void syncToCompactMachine(CompactMachine compactMachine)
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
