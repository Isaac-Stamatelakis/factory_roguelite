using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeArray
{
    protected object[,] array;
    private int size;
    public int Size {get{return size;}}
    
    public RangeArray(int size) {
        this.size = size;
        initalizeArray(size);
    }

    protected virtual void initalizeArray(int size) {
        array = new object[size,size];
    }
    
    public void set(Interval xInterval, Interval yInterval, object value) {
        for (int x = (int)Mathf.Max(0,xInterval.LowerBound); x <= (int) Mathf.Min(Size-1, xInterval.UpperBound); x ++) {
            for (int y = (int)Mathf.Max(0,yInterval.LowerBound); y <= (int) Mathf.Min(Size-1, yInterval.UpperBound); y ++) {
                array[x,y] = value;
            }
        }
    }
    public void set(IntervalVector intervalVector,object value) {
        set(intervalVector.X,intervalVector.Y,value);
    }

    public void delete(Interval xInterval, Interval yInterval) {
        set(xInterval,yInterval,null);
    }
    public void delete(IntervalVector intervalVector) {
        delete(intervalVector.X,intervalVector.Y);
    }
    public object get(int x, int y) {
        if (inBounds(x,y)) {
            return array[x,y];
        }
        return null;
    }
    public bool inBounds(int x, int y) {
        return (x < Size && y < Size);
    }
}

