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

    public bool contains(Vector2Int position) {
        return position.x >= X.LowerBound && position.x <= X.UpperBound && position.y >= Y.LowerBound && position.y <= Y.UpperBound;
    }
    public void add(Vector2Int position) {
        X.LowerBound += position.x;
        X.UpperBound += position.x;
        Y.LowerBound += position.y;
        Y.UpperBound += position.y;
    }
    public Vector2Int getSize() {
        return new Vector2Int(Mathf.Abs(X.LowerBound-X.UpperBound)+1,Mathf.Abs(Y.LowerBound-Y.UpperBound)+1);
    }

    public override string ToString()
    {
        return "X:[" + X.LowerBound + "," + X.UpperBound + "], Y:[" + Y.LowerBound + "," + Y.UpperBound + "]";
    }
}