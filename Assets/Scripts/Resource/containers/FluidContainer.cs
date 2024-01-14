using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidContainer : MatterContainer
{
    public FluidContainer(List<Matter> matter) : base(matter){
        
    }
    public bool valid(Matter value) {
        return value.state == null || value.state is Fluid;
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
