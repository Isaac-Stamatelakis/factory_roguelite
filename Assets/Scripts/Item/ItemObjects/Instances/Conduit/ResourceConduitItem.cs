using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items {
    [CreateAssetMenu(fileName ="I~New Resource Conduit",menuName="Item/Instances/Conduit/Resource")]
    public class ResourceConduitItem : ConduitItem {
        public ResourceConduitType type;
        public uint maxSpeed;

        public override ConduitType GetConduitType()
        {
            return (ConduitType) type;
        }
    }
}

