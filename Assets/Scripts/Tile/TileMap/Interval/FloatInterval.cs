using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatIntervalVector
{
    private Interval<float> x;
    public Interval<float> X {get{return x;} set{x=value;}}
    private Interval<float> y;
    public Interval<float> Y {get{return y;} set{y=value;}}
    public FloatIntervalVector(Interval<float> x, Interval<float> y) {
        this.x = x;
        this.y = y;
    }

    public override string ToString()
    {
        return "X:[" + X.LowerBound + "," + X.UpperBound + "], Y:[" + Y.LowerBound + "," + Y.UpperBound + "]";
    }
}