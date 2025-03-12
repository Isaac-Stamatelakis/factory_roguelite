using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ICaveDistributor
{
    public void Distribute(SeralizedWorldData worldData,int width, int height, Vector2Int bottomLeftCorner);
}
