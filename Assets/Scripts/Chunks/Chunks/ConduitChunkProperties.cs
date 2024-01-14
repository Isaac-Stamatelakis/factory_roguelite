using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class DynamicConduitChunkProperties : DynamicChunkProperties
{
    protected IEnumerator AddConduitsToContainer(ChunkData chunkConduitData,string containerName) {
        ConduitTileMap conduitTileMap = Global.findChild(transform.parent.parent.transform, containerName).GetComponent<ConduitTileMap>();
        Coroutine a = StartCoroutine(conduitTileMap.load(chunkConduitData, this.chunkPosition));
        yield return a;

    }
    protected ChunkData deseralizeConduitChunkTileData(SeralizedChunkConduitData seralizedChunkConduitData) {
        ChunkData chunkConduitData = new ChunkData();
        chunkConduitData.data = new List<List<IdData>>();
        for (int xIter = 0; xIter < Global.ChunkSize; xIter ++) {
            List<IdData> conduitDataList = new List<IdData>();
            for (int yIter = 0; yIter < Global.ChunkSize; yIter ++) {
                int id = seralizedChunkConduitData.ids[xIter][yIter];
                if (id > 0) {
                    IdData idData = IdDataMap.getInstance().GetIdData(id);
                    if (idData is ConduitData) {
                        ConduitData conduitData = (ConduitData) idData;
                        conduitData.conduitOptions = seralizedChunkConduitData.conduitOptions[xIter][yIter];
                        conduitDataList.Add(conduitData);
                    } else {
                        conduitDataList.Add(null);
                    }
                    
                } else {
                    conduitDataList.Add(null);
                }  
            }
            chunkConduitData.data.Add(conduitDataList);
        }
        return chunkConduitData;
    }
    protected override IEnumerator fullLoadChunkCoroutine()
    {
        Coroutine e = StartCoroutine(AddConduitsToContainer(deseralizeConduitChunkTileData((SeralizedChunkConduitData) jsonData.get("EnergyConduits")),"EnergyConduits"));
        Coroutine f = StartCoroutine(AddConduitsToContainer(deseralizeConduitChunkTileData((SeralizedChunkConduitData) jsonData.get("ItemConduits")),"ItemConduits"));
        Coroutine g = StartCoroutine(AddConduitsToContainer(deseralizeConduitChunkTileData((SeralizedChunkConduitData) jsonData.get("FluidConduits")),"FluidConduits"));
        Coroutine h = StartCoroutine(AddConduitsToContainer(deseralizeConduitChunkTileData((SeralizedChunkConduitData) jsonData.get("SignalConduits")),"SignalConduits"));
        yield return base.fullLoadChunkCoroutine();
        
    }

    protected override void saveContainers()
    {
        
        base.saveContainers();
        saveConduitContainer("EnergyConduits");
        saveConduitContainer("ItemConduits");
        saveConduitContainer("FluidConduits");
        saveConduitContainer("SignalConduits");

    }

    private void saveConduitContainer(string containerName) {
        ConduitTileMap conduitTileMap = Global.findChild(transform.parent.parent.transform, containerName).GetComponent<ConduitTileMap>();
        ((SeralizedChunkConduitData) jsonData.get(containerName)).ids = conduitTileMap.getTileIds(chunkPosition);
        ((SeralizedChunkConduitData) jsonData.get(containerName)).conduitOptions = conduitTileMap.getConduitOptions(chunkPosition);
        conduitTileMap.instantlyRemoveChunk(chunkPosition);
    }
}
