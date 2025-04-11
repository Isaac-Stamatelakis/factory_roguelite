using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEntity.Instances.CompactMachines {
    public interface ICompactMachineInteractable : ISoftLoadableTileEntity
    {
        public void SyncToCompactMachine(CompactMachineInstance compactMachine);
    }

    public interface ICompactMachineConduitPort : ICompactMachineInteractable, IConduitInteractable
    {
        public ConduitType GetConduitType();
        public CompactMachinePortType GetPortType();
    }
    public interface ICompactMachine
    {
        public void PlaceInitializeWithHash(string hash);
        public int Depth {get; set;}
    }

}
