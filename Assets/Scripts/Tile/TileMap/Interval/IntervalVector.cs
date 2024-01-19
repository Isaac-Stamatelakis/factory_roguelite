using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntervalVector
{
    private Interval<int> x;
    public Interval<int> X {get{return x;} set{x=value;}}
    private Interval<int> y;
    public Interval<int> Y {get{return y;} set{y=value;}}
    public IntervalVector(Interval<int> x, Interval<int> y) {
        this.x = x;
        this.y = y;
    }
}