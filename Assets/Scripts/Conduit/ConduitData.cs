using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConduitData : PlacedItemObject<ConduitItem>
{
    public ConduitOptions conduitOptions;

    public ConduitData(ConduitItem itemObject) : base(itemObject)
    {
    }
}
