using System;
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
        if (position.x < x.LowerBound) {
            x.LowerBound = position.x;
        } else if (position.x > x.UpperBound) {
            x.UpperBound = position.x;
        }
        if (position.y < y.LowerBound) {
            y.LowerBound = position.y;
        } else if (position.y > y.UpperBound) {
            y.UpperBound = position.y;
        }
    }
    public Vector2Int getSize() {
        return new Vector2Int(Mathf.Abs(X.LowerBound-X.UpperBound)+1,Mathf.Abs(Y.LowerBound-Y.UpperBound)+1);
    }

    public override string ToString()
    {
        return "X:[" + X.LowerBound + "," + X.UpperBound + "], Y:[" + Y.LowerBound + "," + Y.UpperBound + "]";
    }
    public static IntervalVector operator *(IntervalVector vector, int scalar)
    {
        return new IntervalVector(new Interval<int>(vector.X.LowerBound*scalar,vector.X.UpperBound*scalar),new Interval<int>(vector.Y.LowerBound*scalar,vector.Y.UpperBound*scalar));
    }

    public static void Iterate(IntervalVector intervalVector, Action<int, int> action)
    {
        for (int x = intervalVector.X.LowerBound; x <= intervalVector.X.UpperBound; x++)
        {
            for (int y = intervalVector.Y.LowerBound; y <= intervalVector.Y.UpperBound; y++)
            {
                action.Invoke(x,y);
            }
        }
    }
    
    public static bool IterateCondition(IntervalVector intervalVector, Func<int, int, bool> action)
    {
        for (int x = intervalVector.X.LowerBound; x <= intervalVector.X.UpperBound; x++)
        {
            for (int y = intervalVector.Y.LowerBound; y <= intervalVector.Y.UpperBound; y++)
            {
                if (!action.Invoke(x, y)) return false;
            }
        }

        return true;
    }
}