using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Items {
    [CreateAssetMenu(fileName ="I~New Signal Conduit Item",menuName="Item/Instances/Conduit/Signal")]
    public class SignalConduitItem : ConduitItem
    {
        public int maxDistance;
        public override ConduitType GetConduitType()
        {
            return ConduitType.Signal;
        }
    }
}

