using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ICaveDistributor
{
    public void distribute(SeralizedWorldData seralizedWorldData,int width, int height, Vector2Int bottomLeftCorner);
}
