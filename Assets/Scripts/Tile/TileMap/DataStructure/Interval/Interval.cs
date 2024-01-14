using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interval : IInterval{
    private int lowerBound;
    public int LowerBound {get{return lowerBound;}}
    private int upperBound;
    public int UpperBound {get{return upperBound;}}
    public Interval(int lowerBound, int upperBound) {
        this.lowerBound = lowerBound;
        this.upperBound = upperBound;
    }
}
