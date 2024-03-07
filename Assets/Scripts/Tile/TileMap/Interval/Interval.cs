using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interval<T> {
    private T lowerBound;
    public T LowerBound {get{return lowerBound;} set => lowerBound = value;}
    private T upperBound;
    public T UpperBound {get{return upperBound;} set => upperBound = value;}
    public Interval(T lowerBound, T upperBound) {
        this.lowerBound = lowerBound;
        this.upperBound = upperBound;
    }
}