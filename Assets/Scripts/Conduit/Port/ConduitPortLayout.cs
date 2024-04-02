using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConduitModule.Ports {
    [CreateAssetMenu(fileName ="New Conduit Port Layout",menuName="Tile Entity/ConduitLayout")]
    public class ConduitPortLayout : ScriptableObject
    {
        public List<TileEntityPort> itemPorts;
        public List<TileEntityPort> fluidPorts;
        public List<TileEntityPort> energyPorts;
        public List<TileEntityPort> signalPorts;
        public List<TileEntityPort> matrixPorts;
    }
}

