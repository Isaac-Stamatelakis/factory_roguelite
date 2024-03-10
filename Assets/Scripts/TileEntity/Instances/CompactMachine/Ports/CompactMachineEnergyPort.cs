using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule.Instances.Storage;
using ConduitModule.Ports;
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

        public ref int getEnergy()
        {
            return ref energy;
        }
        
        /// <summary>
        /// Allows unbounded throughput but has no storage
        /// </summary>
        public int insertEnergy(int energy)
        {
            if (energy > 0) {
                return 0;
            }   
            this.energy = energy;
            return 0;
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
            energy = JsonConvert.DeserializeObject<int>(data);
        }
    }

}
