using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DynamicChunkProperties : ChunkProperties
{
    public override IEnumerator unfullLoadChunk() {
        if (!fullLoaded) {
            yield return null;
        }
        gameObject.name = gameObject.name.Split("|")[0];
        gameObject.layer = LayerMask.NameToLayer("UnloadedChunk");
        fullLoaded = false;
        saveContainers();
        yield return destroyContainers();
    }

    protected virtual void saveContainers() {
        saveEntities();
        saveTileContainer("TileBlocks");
        saveTileContainer("TileBackgrounds");
        saveTileContainer("TileObjects");
        
    }
    protected void unfullLoadTileEntities() {
        Global.findChild(transform,"TileEntities").GetComponent<TileEntityContainerController>().unFullLoadTileEntities();
    }

    protected void saveTileEntities() {
        TileEntityContainerController controller = Global.findChild(transform,"TileEntities").GetComponent<TileEntityContainerController>();
        controller.saveAllTileEntities();
        ((SeralizedChunkTileData) jsonData.get("TileBlocks")).sTileEntityOptions = controller.getDynamicTileEntityOptions("TileBlocks");
        ((SeralizedChunkTileData) jsonData.get("TileBackgrounds")).sTileEntityOptions = controller.getDynamicTileEntityOptions("TileBackgrounds");
        ((SeralizedChunkTileData) jsonData.get("TileObjects")).sTileEntityOptions = controller.getDynamicTileEntityOptions("TileObjects");
    }

    protected void saveTileContainer(string containerName) {
        SeralizedChunkTileData seralizedChunkTileData = (SeralizedChunkTileData) jsonData.get(containerName);
        TileGridMap tileBlockGridMap = Global.findChild(transform.parent.parent.transform, containerName).GetComponent<TileGridMap>();
        if (tileBlockGridMap.containsChunk(chunkPosition)) {
            seralizedChunkTileData.ids = tileBlockGridMap.getTileIds(chunkPosition);
            seralizedChunkTileData.sTileOptions = tileBlockGridMap.getSeralizedTileOptions(chunkPosition);
            jsonData.set(containerName,seralizedChunkTileData);
        } 
    }
    /**
    Loads only gameobjects required for machine processing to function.
    ie, tileEntities and conduits.
    **/
    

    public void saveToJson() {
        if (fullLoaded) {
            saveContainers();
            instantlyDestroyContainers();
            saveTileEntities();
            ChunkIO.writeChunk(jsonData,this);
        } else {
            saveTileEntities();
            ChunkIO.writeChunk(jsonData,this);
        }
    }



    protected virtual void saveEntities() {
        GameObject entityContainer = Global.findChild(transform,"Entities");
        if (entityContainer != null) {
            jsonData.set("Entities",EntityHelper.entityContainerToList(entityContainer.transform));
            GameObject.Destroy(entityContainer.gameObject);
        }
        
    }
}
