using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class DynamicConduitChunkProperties : DynamicChunkProperties
{
    protected IEnumerator AddConduitsToContainer(ChunkData<ConduitData> chunkConduitData,string containerName,int sectionAmount, Vector2Int direction) {
        ConduitTileMap conduitTileMap = Global.findChild(transform.parent.parent.transform, containerName).GetComponent<ConduitTileMap>();
        Coroutine a = StartCoroutine(conduitTileMap.load(chunkConduitData, this.chunkPosition,sectionAmount,direction));
        yield return a;

    }
    protected ChunkData<ConduitData> deseralizeConduitChunkTileData(SeralizedChunkConduitData seralizedChunkConduitData) {
        ChunkData<ConduitData> chunkConduitData = new ChunkData<ConduitData>();
        chunkConduitData.data = new List<List<ConduitData>>();
        ItemRegistry itemRegister = ItemRegistry.getInstance();
        for (int xIter = 0; xIter < Global.ChunkSize; xIter ++) {
            List<ConduitData> conduitDataList = new List<ConduitData>();
            for (int yIter = 0; yIter < Global.ChunkSize; yIter ++) {
                string id = seralizedChunkConduitData.ids[xIter][yIter];
                if (id != null) {
                    ConduitItem conduitItem = itemRegister.GetConduitItem(id);
                    ConduitData conduitData = new ConduitData(itemObject: conduitItem);
                    // TODO add deseralize conduit options
                    conduitDataList.Add(conduitData);
                } else {
                    conduitDataList.Add(null);
                }  
            }
            chunkConduitData.data.Add(conduitDataList);
        }
        return chunkConduitData;
    }
    protected override IEnumerator fullLoadChunkCoroutine(int sectionAmount, Vector2Int direction)
    {
        Coroutine e = StartCoroutine(AddConduitsToContainer(
            deseralizeConduitChunkTileData((SeralizedChunkConduitData) jsonData.get("EnergyConduits")),
            "EnergyConduits",
            sectionAmount,
            direction
        ));
        Coroutine f = StartCoroutine(AddConduitsToContainer(
            deseralizeConduitChunkTileData((SeralizedChunkConduitData) jsonData.get("ItemConduits")),
            "ItemConduits",
            sectionAmount,
            direction
        ));
        Coroutine g = StartCoroutine(AddConduitsToContainer(
            deseralizeConduitChunkTileData((SeralizedChunkConduitData) jsonData.get("FluidConduits")),
            "FluidConduits",
            sectionAmount,
            direction
        ));
        Coroutine h = StartCoroutine(AddConduitsToContainer(
            deseralizeConduitChunkTileData((SeralizedChunkConduitData) jsonData.get("SignalConduits")),
            "SignalConduits",
            sectionAmount,
            direction
        ));
        yield return base.fullLoadChunkCoroutine(sectionAmount, direction);
        
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
