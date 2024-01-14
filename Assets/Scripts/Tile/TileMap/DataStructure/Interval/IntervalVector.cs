using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntervalVector
{
    private Interval x;
    public Interval X {get{return x;} set{x=value;}}
    private Interval y;
    public Interval Y {get{return y;} set{y=value;}}
    public IntervalVector(Interval x, Interval y) {
        this.x = x;
        this.y = y;
    }
}

