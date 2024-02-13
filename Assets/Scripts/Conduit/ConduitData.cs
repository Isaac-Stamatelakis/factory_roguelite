using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConduitData : PlacedItemObject<ConduitItem>
{
    public IConduitOptions conduitOptions;

    public ConduitData(ConduitItem itemObject) : base(itemObject)
    {
        
    }
}
