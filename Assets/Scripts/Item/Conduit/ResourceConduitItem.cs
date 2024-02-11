using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="I~New Resource Conduit",menuName="Item Register/Conduit/Resource")]
public class ResourceConduitItem : ConduitItem {
    public ResourceConduitType type;
    public int maxSpeed;

    public override ConduitType getType()
    {
        return (ConduitType) type;
    }
}
