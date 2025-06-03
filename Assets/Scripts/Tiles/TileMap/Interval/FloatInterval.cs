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

    public static IntervalVector ToIntervalVector(FloatIntervalVector floatIntervalVector)
    {
        return new IntervalVector(new Interval<int>((int)floatIntervalVector.X.LowerBound, (int)floatIntervalVector.X.UpperBound), new Interval<int>((int)floatIntervalVector.Y.LowerBound, (int)floatIntervalVector.Y.UpperBound));
    }
    public static IntervalVector ToCellIntervalVector(FloatIntervalVector floatIntervalVector)
    {
        Vector2Int lower = Global.WorldToCell(new Vector2(floatIntervalVector.X.LowerBound,floatIntervalVector.y.UpperBound));
        Vector2Int upper = Global.WorldToCell(new Vector2(floatIntervalVector.X.UpperBound, floatIntervalVector.y.UpperBound));
        return new IntervalVector(new Interval<int>(lower.x,upper.x), new Interval<int>(lower.y, upper.y));
    }
}

