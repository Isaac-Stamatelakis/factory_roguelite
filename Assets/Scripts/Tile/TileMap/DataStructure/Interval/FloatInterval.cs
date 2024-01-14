using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatInterval : IInterval
{
    private float lowerBound;
    public float LowerBound {get{return lowerBound;}}
    private float upperBound;
    public float UpperBound {get{return upperBound;}}
    public FloatInterval(float lowerBound, float upperBound) {
        this.lowerBound = lowerBound;
        this.upperBound = upperBound;
    }
}
