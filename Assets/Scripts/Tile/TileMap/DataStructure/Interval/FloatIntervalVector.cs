using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatIntervalVector
{
    private FloatInterval x;
    public FloatInterval X {get{return x;} set{x=value;}}
    private FloatInterval y;
    public FloatInterval Y {get{return y;} set{y=value;}}
    public FloatIntervalVector(FloatInterval x, FloatInterval y) {
        this.x = x;
        this.y = y;
    }
}