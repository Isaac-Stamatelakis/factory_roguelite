using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEntity.Instances.CompactMachines {
    public interface ICompactMachineInteractable : ISoftLoadableTileEntity
    {
        public void SyncToCompactMachine(CompactMachineInstance compactMachine);
    }

    public interface ICompactMachine
    {
        public void PlaceInitializeWithHash(string hash);
    }

}
