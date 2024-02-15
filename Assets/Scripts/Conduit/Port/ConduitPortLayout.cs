using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConduitModule.Ports {
    [CreateAssetMenu(fileName ="New Conduit Port Layout",menuName="Tile Entity/ConduitLayout")]
    public class ConduitPortLayout : ScriptableObject
    {
        public List<ConduitPortData> itemPorts;
        public List<ConduitPortData> fluidPorts;
        public List<ConduitPortData> energyPorts;
        public List<ConduitPortData> signalPorts;
    }
}

