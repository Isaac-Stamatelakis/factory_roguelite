using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileValueRangeArray : RangeArray
{
    
    public TileValueRangeArray(int size) : base(size) {
        initalizeArray(size);
    }

    protected override void initalizeArray(int size) {
        array = new TileValue[size,size];
    }
    /// <summary>
    /// Returns a list of tileValues which are in the array.
    /// Duplicate objects in the array map to the same point.
    /// </summary>
    public List<List<TileValue>> toNestedList() {
        List<List<TileValue>> nestedList = new List<List<TileValue>>();
        for (int x = 0; x < Size; x ++) {
            List<TileValue> list = new List<TileValue>();
            for (int y = 0; y < Size; y ++) {
                list.Add((TileValue) array[x,y]);
            }
            nestedList.Add(list);
        }
        return nestedList;
    }
    
}
