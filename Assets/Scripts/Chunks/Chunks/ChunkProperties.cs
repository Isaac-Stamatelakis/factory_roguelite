using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ChunkProperties : MonoBehaviour
{
    protected JsonData jsonData;
    [SerializeField]
    protected bool fullLoaded = false;
    [SerializeField]
    protected bool scheduledForUnloading = false;
    public bool ScheduledForUnloading {set{scheduledForUnloading=value;} get{return scheduledForUnloading;}}
    public bool FullLoaded {set{fullLoaded = value;} get{return fullLoaded;}}
    protected Vector2Int chunkPosition;
    public Vector2Int ChunkPosition {get{return chunkPosition;}}
    protected int dim;
    public int Dim {get{return dim;}}
    protected Transform entityContainer;
    public Transform EntityContainer {get{return entityContainer;}}
    // Start is called before the first frame update
    public virtual void Start()
    {
        
    }

    // Update is called once per frame
    public virtual void Update()
    {
        
    }

    public IEnumerator fullLoadChunk(int sectionAmount, Vector2Int direction) {
        if (fullLoaded) {
            yield return null;
        }   
        this.fullLoaded = true;
        yield return StartCoroutine(fullLoadChunkCoroutine(sectionAmount,direction));

    }
    public virtual void initalize(int dim, Vector2Int chunkPosition, JsonData jsonData, Transform closedSystemTransform) {
        // Set variables
        this.dim = dim;
        this.chunkPosition = chunkPosition;
        this.jsonData = jsonData;
        transform.localPosition = new Vector3(8f*chunkPosition.x+4f,8f*chunkPosition.y+4f,0);
        gameObject.AddComponent<BoxCollider2D>();
        gameObject.GetComponent<BoxCollider2D>().size = new Vector2(Global.ChunkSize/2, Global.ChunkSize/2);
        gameObject.layer = LayerMask.NameToLayer("UnloadedChunk");
        gameObject.transform.SetParent(Global.findChild(closedSystemTransform,"Chunks").transform);
        StartCoroutine(softLoadChunk(jsonData));
    }

    protected virtual IEnumerator fullLoadChunkCoroutine(int sectionAmount, Vector2Int direction) {
        Coroutine a = StartCoroutine(addTilesToContainer(
            deseralizeChunkTileData((SeralizedChunkTileData) jsonData.get("TileBlocks")),
            "TileBlocks",
            sectionAmount,
            direction
        ));
        Coroutine b = StartCoroutine(addTilesToContainer(deseralizeChunkTileData(
            (SeralizedChunkTileData) jsonData.get("TileBackgrounds")),
            "TileBackgrounds",
            sectionAmount,
            direction
        ));
        Coroutine c = StartCoroutine(addTilesToContainer(
            deseralizeChunkTileData((SeralizedChunkTileData) jsonData.get("TileObjects")),
            "TileObjects",
            sectionAmount,
            direction
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
        fullLoaded = false;
        scheduledForUnloading = false;
        yield return destroyContainers();
    }

    public virtual void instantlyUnFullLoadChunk() {
        if (!fullLoaded) {
            return;
        }
        gameObject.name = gameObject.name.Split("|")[0];
        gameObject.layer = LayerMask.NameToLayer("UnloadedChunk");
        fullLoaded = false;
        instantlyDestroyContainers();
    }

    protected IEnumerator addTilesToContainer(ChunkData<TileData> tileData,string containerName,int sectionAmount, Vector2Int direction) {
        TileGridMap tileGridMap = Global.findChild(transform.parent.parent.transform, containerName).GetComponent<TileGridMap>();
        Coroutine a = StartCoroutine(tileGridMap.load(tileData, chunkPosition,sectionAmount,direction));
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
        */
    }

    protected ChunkData<TileData> deseralizeChunkTileData(SeralizedChunkTileData seralizedChunkTileData) {
        List<List<TileData>> nestedTileDataList = new List<List<TileData>>();
        ItemRegistry itemRegister = ItemRegistry.getInstance();
        for (int xIter = 0; xIter < 16; xIter ++) {
            List<TileData> tileDataList = new List<TileData>();
            for (int yIter = 0; yIter < 16; yIter ++) {
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
}
