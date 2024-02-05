using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Cave
{
    [SerializeField]
    public List<CaveArea> areas = new List<CaveArea>();
    public UnityEngine.Vector2Int getChunkDimensions() {
        int index = 0;
        if (areas.Count < 0) {
            return UnityEngine.Vector2Int.zero;
        }

        int minXChunk = areas[index].xInterval.x;
        int maxXChunk = areas[index].xInterval.y;
        int minYChunk = areas[index].yInterval.x;
        int maxYChunk = areas[index].yInterval.y;
        index ++;
        while (index < areas.Count) {
            CaveArea caveArea = areas[index];
            minXChunk = Mathf.Min(minXChunk,caveArea.xInterval.x);
            maxXChunk = Mathf.Max(maxXChunk,caveArea.xInterval.y);
            minYChunk = Mathf.Min(minYChunk,caveArea.yInterval.x);
            maxYChunk = Mathf.Max(maxYChunk,caveArea.yInterval.y);
            index ++;
        }
        return new UnityEngine.Vector2Int(Mathf.Abs(maxXChunk - minXChunk + 1), Mathf.Abs(maxYChunk - minYChunk + 1));
    }

    public IntervalVector getCoveredArea() {
        int index = 0;
        if (areas.Count < 0) {
            return new IntervalVector(
                new Interval<int>(0,0),
                new Interval<int>(0,0)
            );
        }

        int minXChunk = areas[index].xInterval.x;
        int maxXChunk = areas[index].xInterval.y;
        int minYChunk = areas[index].yInterval.x;
        int maxYChunk = areas[index].yInterval.y;
        index ++;
        while (index < areas.Count) {
            CaveArea caveArea = areas[index];
            minXChunk = Mathf.Min(minXChunk,caveArea.xInterval.x);
            maxXChunk = Mathf.Max(maxXChunk,caveArea.xInterval.y);
            minYChunk = Mathf.Min(minYChunk,caveArea.yInterval.x);
            maxYChunk = Mathf.Max(maxYChunk,caveArea.yInterval.y);
            index ++;
        }
        return new IntervalVector(new Interval<int>(minXChunk, maxXChunk),new Interval<int>(minYChunk,maxYChunk));
    }
}
[System.Serializable]

public class CaveArea {
    public UnityEngine.Vector2Int xInterval;
    public UnityEngine.Vector2Int yInterval;
    public int cellRadius;
    public int cellNeighboorCount;
    public float fillPercent;
    public string defaultBlockID;
    public int smoothIterations;
    public CaveArea(
        UnityEngine.Vector2Int xInterval, 
        UnityEngine.Vector2Int yInterval, 
        string defaultBlockID,
        int cellRadius, 
        int cellNeighboorCount, 
        float fillPercent, 
        int smoothIterations
        
        ) {
        this.xInterval = xInterval;
        this.yInterval = yInterval;
        this.cellRadius = cellRadius;
        this.cellNeighboorCount = cellNeighboorCount;
        this.fillPercent = fillPercent;
        this.smoothIterations = smoothIterations;
        this.defaultBlockID = defaultBlockID;
    }

    public UnityEngine.Vector2Int getSize() {
        return new UnityEngine.Vector2Int(Mathf.Abs(xInterval.y - xInterval.x+1), Mathf.Abs(yInterval.y - yInterval.x+1));
    }
}

public class ProceduralTileGeneration {
    public string id;
    public string frequency;
    public string size;
}

public class ProceduralStructure {
    
}