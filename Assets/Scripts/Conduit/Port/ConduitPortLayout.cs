using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Conduits.Ports {
    [CreateAssetMenu(fileName ="New Conduit Port Layout",menuName="Tile Entity/Layouts/Conduit")]
    public class ConduitPortLayout : ScriptableObject
    {
        public List<TileEntityPortData> itemPorts;
        public List<TileEntityPortData> fluidPorts;
        public List<TileEntityPortData> energyPorts;
        public List<TileEntityPortData> signalPorts;
        public List<TileEntityPortData> matrixPorts;
    }
}

