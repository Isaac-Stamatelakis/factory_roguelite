using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemContainer : MatterContainer
{
    public ItemContainer(List<Matter> matter) : base(matter){
        
    }
    protected bool valid(Matter value) {
        return value == null || value.state is Solid;
    }
    public override void set(int n, Matter value)
    {
        if (valid(value)) {
            base.set(n, value);
        }
    }
    public override void add(Matter value)
    {
        if (valid(value)) {
            base.add(value);
        }        
    }
}
