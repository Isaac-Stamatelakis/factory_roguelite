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

    public bool inBounds(Vector2Int position) {
        return position.x >= X.LowerBound && position.x <= X.UpperBound && position.y >= Y.LowerBound && position.y <= Y.UpperBound;
    }
}