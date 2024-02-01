using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public interface IChunk {
    public List<List<IChunkPartition>> getChunkPartitions();
    public List<ChunkPartitionData> getChunkPartitionData();
    public List<IChunkPartition> getUnloadedPartitionsCloseTo(Vector2Int target);
    /// <summary>
    /// Deletes all chunk partitions
    /// </summary>
    public void unload();
    public float distanceFrom(Vector2Int target);
    public bool inRange(Vector2Int target, int xRange, int yRange);
    public bool isChunkLoaded();
    
}

public interface ISerizable {
    public void serialze();
}
public class Chunk : MonoBehaviour, IChunk
{
    protected List<List<IChunkPartition>> partitions;

    [SerializeField]
    /// <summary>
    /// a chunk is soft loaded if all tile entity machines inside of it are loaded
    /// </summary>
    protected bool softLoaded = false;
    [SerializeField]
    /// <summary>
    /// a chunk is chunk loaded if it remains softloaded whilst the player is far away
    /// </summary>
    protected bool chunkLoaded = false;
    protected Vector2Int chunkPosition; 
    public Vector2Int ChunkPosition {get{return chunkPosition;}}
    protected int dim;
    public int Dim {get{return dim;}}
    protected Transform entityContainer;
    public Transform EntityContainer {get{return entityContainer;}}

    /*
    public IEnumerator fullLoadChunk(int sectionAmount, double angle) {
        if (fullLoaded) {
            yield return null;
        }   
        this.fullLoaded = true;
        yield return StartCoroutine(fullLoadChunkCoroutine(sectionAmount,angle));

    }
    */

    public float distanceFrom(Vector2Int target)
    {
        return Mathf.Pow(target.x-chunkPosition.x,2) + Mathf.Pow(target.y-chunkPosition.y,2);
    }
    public virtual void initalize(int dim, List<ChunkPartitionData> chunkPartitionDataList, Vector2Int chunkPosition, ClosedChunkSystem closedSystemTransform) {
        this.dim = dim;
        this.chunkPosition = chunkPosition;
        this.partitions = new List<List<IChunkPartition>>();
        transform.SetParent(closedSystemTransform.ChunkContainerTransform);
        generatePartitions(chunkPartitionDataList);
        transform.localPosition = new Vector3(Global.ChunkSize/2*(chunkPosition.x+0.5F),Global.ChunkSize/2*(chunkPosition.y+0.5F),0);
    }

    protected void generatePartitions(List<ChunkPartitionData> chunkPartitionDataList) {
        for (int x = 0; x < Global.PartitionsPerChunk; x ++) {
            List<IChunkPartition> chunkPartitions = new List<IChunkPartition>();
            for (int y = 0; y < Global.PartitionsPerChunk; y ++) {
                generatePartitionCollider(x,y);
                chunkPartitions.Add(generatePartition(chunkPartitionDataList[x*Global.PartitionsPerChunk + y], new Vector2Int(x,y)));
            }
            partitions.Add(chunkPartitions);
        }
    }
    /// <summary>
    /// Generates colliders for partitions so that entities don't fall through them
    /// </summary>
    protected void generatePartitionCollider(int x, int y) {

    }
    /// <summary>
    /// Generates a partition
    /// </summary>
    protected virtual IChunkPartition generatePartition(ChunkPartitionData data, Vector2Int position) {
        if (data is SerializedTileData) {
            return new TileChunkPartition<SerializedTileData>((SerializedTileData) data,position,this);
        } else if (data is SerializedTileConduitData) {
            return new ConduitChunkPartition<SerializedTileConduitData>((SerializedTileConduitData) data,position,this);
        }
        return null;
    }
    public List<ChunkPartitionData> getChunkPartitionData()
    {
        List<ChunkPartitionData> dataList = new List<ChunkPartitionData>();
        foreach (List<IChunkPartition> chunkPartitionList in partitions) {
            foreach (IChunkPartition chunkPartition in chunkPartitionList) {
                dataList.Add(chunkPartition.getData());
            }
        }
        return dataList;
    }

    public virtual void unload()
    {
        ChunkIO.writeChunk(this);
        GameObject.Destroy(gameObject);

    }

    public List<List<IChunkPartition>> getChunkPartitions()
    {
        return this.partitions;
    }

    public List<IChunkPartition> getUnloadedPartitionsCloseTo(Vector2Int target)
    {
        List<IChunkPartition> close = new List<IChunkPartition>();
        foreach (List<IChunkPartition> partitionList in partitions) {
            foreach (IChunkPartition partition in partitionList) {
                if (!partition.getLoaded() && partition.inRange(target,Global.ChunkPartitionLoadRange.x,Global.ChunkPartitionLoadRange.y)) {
                    close.Add(partition);
                } 
            }
        }
        return close;
    }

    public bool inRange(Vector2Int target, int xRange, int yRange)
    {
        return Mathf.Abs(target.x-chunkPosition.x) <= xRange && Mathf.Abs(target.y-chunkPosition.y) <= yRange;
    }

    public bool isChunkLoaded()
    {
        return this.chunkLoaded;
    }
    /*
protected virtual IEnumerator fullLoadChunkCoroutine(int sectionAmount, double angle) {

StartCoroutine(softLoadChunk(jsonData));
Coroutine a = StartCoroutine(addTilesToContainer(
deseralizeChunkTileData((SeralizedChunkTileData) jsonData.get("TileBlocks")),
"TileBlocks",
sectionAmount,
angle
));
Coroutine b = StartCoroutine(addTilesToContainer(deseralizeChunkTileData(
(SeralizedChunkTileData) jsonData.get("TileBackgrounds")),
"TileBackgrounds",
sectionAmount,
angle
));
Coroutine c = StartCoroutine(addTilesToContainer(
deseralizeChunkTileData((SeralizedChunkTileData) jsonData.get("TileObjects")),
"TileObjects",
sectionAmount,
angle
));

yield return a;
yield return b;
yield return c;

GameObject tileEntityContainer = Global.findChild(transform,"TileEntities");
Coroutine d = StartCoroutine(tileEntityContainer.GetComponent<TileEntityContainerController>().fullLoadAllTileEntities());

yield return d;

initEntityContainer(jsonData);

fullLoaded = true;
gameObject.name = gameObject.name +"|FullLoaded";
gameObject.layer = LayerMask.NameToLayer("Chunk");
yield return null;
}


public IEnumerator softLoadChunk(JsonData jsonData) {
GameObject tileEntities = new GameObject();
tileEntities.transform.localPosition = new Vector3(transform.position.x-4,transform.position.y-4, 0);
tileEntities.name="TileEntities";
tileEntities.transform.SetParent(transform);
tileEntities.AddComponent<TileEntityContainerController>();
Global.setStatic(tileEntities);
Coroutine a = StartCoroutine(initTileEntitities((SeralizedChunkTileData) jsonData.get("TileBlocks"),"TileBlocks"));
Coroutine b = StartCoroutine(initTileEntitities((SeralizedChunkTileData) jsonData.get("TileBackgrounds"),"TileBackgrounds"));
Coroutine c = StartCoroutine(initTileEntitities((SeralizedChunkTileData) jsonData.get("TileObjects"),"TileObjects"));
yield return a;
yield return b;
yield return c;
}

protected IEnumerator initTileEntitities(SeralizedChunkTileData chunkTileData, string tileContainerName) {
Transform tileEntityContainer = Global.findChild(transform,"TileEntities").transform;
ItemRegistry itemRegister = ItemRegistry.getInstance();
for (int xIter = 0; xIter < Global.ChunkSize; xIter ++) {
for (int yIter = 0; yIter < Global.ChunkSize; yIter ++) {
string id = chunkTileData.ids[xIter][yIter];
if (id == null) {
continue;
}
TileItem tileItem = itemRegister.getTileItem(id);
if (tileItem!= null && tileItem.tileEntityOptions.Count != 0) {
TileEntityFactory.softLoadTileEntity(tileItem,chunkTileData.sTileEntityOptions[xIter][yIter],tileEntityContainer,tileContainerName,new Vector2Int(xIter,yIter));
}
}
yield return new WaitForSeconds(0.01f);
}
yield return null;
}

public virtual IEnumerator unfullLoadChunk() {
if (!fullLoaded) {
yield return null;
}
gameObject.name = gameObject.name.Split("|")[0];
gameObject.layer = LayerMask.NameToLayer("UnloadedChunk");
scheduledForUnloading = false;
yield return destroyContainers();
string chunkName = "chunk[" + chunkPosition.x + "," + chunkPosition.y + "].json";
string filePath = Application.dataPath + "/Resources/worlds/" + Global.WorldName + "/Chunks/dim" + dim + "/" + chunkName;
File.WriteAllText(filePath, Newtonsoft.Json.JsonConvert.SerializeObject(jsonData));
jsonData = null;    
fullLoaded = false;

}

public virtual void instantlyUnFullLoadChunk() {
if (!fullLoaded) {
return;
}
gameObject.name = gameObject.name.Split("|")[0];
gameObject.layer = LayerMask.NameToLayer("UnloadedChunk");
fullLoaded = false;
scheduledForUnloading = false;
instantlyDestroyContainers();
}

protected IEnumerator addTilesToContainer(ChunkData<TileData> tileData,string containerName,int sectionAmount, double angle) {
TileGridMap tileGridMap = Global.findChild(transform.parent.parent.transform, containerName).GetComponent<TileGridMap>();
Coroutine a = StartCoroutine(tileGridMap.load(tileData, chunkPosition,sectionAmount,angle));
yield return a;

}

protected virtual IEnumerator destroyContainers() {
TileGridMap tileBlockGridMap = Global.findChild(transform.parent.parent.transform, "TileBlocks").GetComponent<TileGridMap>();
Coroutine a = StartCoroutine(tileBlockGridMap.removeChunk(chunkPosition));

TileGridMap tileBackgroundGripMap = Global.findChild(transform.parent.parent.transform, "TileBackgrounds").GetComponent<TileGridMap>();
Coroutine b = StartCoroutine(tileBackgroundGripMap.removeChunk(chunkPosition));

TileGridMap tileObjectGridMap = Global.findChild(transform.parent.parent.transform, "TileObjects").GetComponent<TileGridMap>();
Coroutine c = StartCoroutine(tileObjectGridMap.removeChunk(chunkPosition));
yield return a;
yield return b;
yield return c;
}

protected virtual void instantlyDestroyContainers() {
TileGridMap tileBlockGridMap = Global.findChild(transform.parent.parent.transform, "TileBlocks").GetComponent<TileGridMap>();
tileBlockGridMap.instantlyRemoveChunk(chunkPosition);

TileGridMap tileBackgroundGripMap = Global.findChild(transform.parent.parent.transform, "TileBackgrounds").GetComponent<TileGridMap>();
tileBackgroundGripMap.instantlyRemoveChunk(chunkPosition);

TileGridMap tileObjectGridMap = Global.findChild(transform.parent.parent.transform, "TileObjects").GetComponent<TileGridMap>();
tileObjectGridMap.instantlyRemoveChunk(chunkPosition);
}

protected void initEntityContainer(JsonData jsonData) {
GameObject entityContainer = new GameObject();
entityContainer.name = "Entities";
entityContainer.transform.SetParent(transform);
this.entityContainer = entityContainer.transform;
List<EntityData> entityDataList = (List<EntityData>) jsonData.get("Entities");
ItemRegistry itemRegister = ItemRegistry.getInstance();
// TODO refactor item entities
/*
foreach (EntityData entityData in entityDataList) {
GameObject entityObject = new GameObject();
if (entityData.tileType != null) {
entityObject.AddComponent<TileItemEntityProperties>();
TileItemEntityProperties itemEntityProperties = entityObject.GetComponent<TileItemEntityProperties>();
itemEntityProperties.itemObject =  itemRegister.getItemObject(id);
itemEntityProperties.initalize(entityData.amount);
itemEntityProperties.setLocation(entityData.x, entityData.y);
itemEntityProperties.setParent(entityContainer.transform);
}
}

}

protected ChunkData<TileData> deseralizeChunkTileData(SeralizedChunkTileData seralizedChunkTileData) {
List<List<TileData>> nestedTileDataList = new List<List<TileData>>();
ItemRegistry itemRegister = ItemRegistry.getInstance();
for (int xIter = 0; xIter < Global.ChunkSize; xIter ++) {
List<TileData> tileDataList = new List<TileData>();
for (int yIter = 0; yIter < Global.ChunkSize; yIter ++) {
string id = seralizedChunkTileData.ids[xIter][yIter];
if (id != null) {
TileItem tileItem = itemRegister.getTileItem(id);
if (tileItem == null) {
tileDataList.Add(null);
} else {
TileData tileData = new TileData(itemObject: tileItem, options: tileItem.getOptions());
Dictionary<string,object> serializedData =  seralizedChunkTileData.sTileOptions[xIter][yIter];
TileEntityOptionFactory.deseralizeOptions(serializedData:serializedData,tileData.options);
tileDataList.Add(tileData); 
}

} else {
tileDataList.Add(null);
}        
}
nestedTileDataList.Add(tileDataList);
}
ChunkData<TileData> chunkTileData = new ChunkData<TileData>() {
data = nestedTileDataList
};
return chunkTileData;
}
*/
}
