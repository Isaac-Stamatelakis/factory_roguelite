using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName ="New Signal Conduit",menuName="Item Register/Conduit/Signal")]
public class SignalConduitItem : ConduitItem
{
    public int maxDistance;
    public override ConduitType getType()
    {
        return ConduitType.Signal;
    }
}
