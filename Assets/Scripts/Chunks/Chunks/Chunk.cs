using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public interface IChunk {
    public List<List<IChunkPartition>> getChunkPartitions();
    public List<ChunkPartitionData> getChunkPartitionData();
    public List<IChunkPartition> getUnloadedPartitionsCloseTo(Vector2Int target);
    public List<IChunkPartition> getLoadedPartitionsFar(Vector2Int target);
    public bool partionsAreAllUnloaded();
    /// <summary>
    /// Deletes all chunk partitions
    /// </summary>
    public void unload();
    public float distanceFrom(Vector2Int target);
    public bool inRange(Vector2Int target, int xRange, int yRange);
    public bool isChunkLoaded();
    public Vector2Int getPosition();
    public int getDim();
    public Transform getEntityContainer();
    public Transform getTileEntityContainer();
    
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
    protected Vector2Int position; 
    protected int dim;
    protected Transform entityContainer;
    protected Transform tileEntityContainer;

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
        return Mathf.Pow(target.x-position.x,2) + Mathf.Pow(target.y-position.y,2);
    }
    public virtual void initalize(int dim, List<ChunkPartitionData> chunkPartitionDataList, Vector2Int chunkPosition, ClosedChunkSystem closedSystemTransform) {
        this.dim = dim;
        this.position = chunkPosition;
        this.partitions = new List<List<IChunkPartition>>();
        transform.SetParent(closedSystemTransform.ChunkContainerTransform);
        generatePartitions(chunkPartitionDataList);
        transform.localPosition = new Vector3(Global.ChunkSize/2*(chunkPosition.x+0.5F),Global.ChunkSize/2*(chunkPosition.y+0.5F),0);

        GameObject tileEntityContainerObject = new GameObject();
        tileEntityContainerObject.name = "TileEntities";
        tileEntityContainer = tileEntityContainerObject.transform;
        tileEntityContainer.transform.SetParent(transform);

        GameObject entityContainerObject = new GameObject();
        entityContainerObject.name = "Entities";
        entityContainer = entityContainerObject.transform;
        entityContainer.transform.SetParent(transform);
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
        return Mathf.Abs(target.x-position.x) <= xRange && Mathf.Abs(target.y-position.y) <= yRange;
    }

    public bool isChunkLoaded()
    {
        return this.chunkLoaded;
    }

    public List<IChunkPartition> getLoadedPartitionsFar(Vector2Int target)
    {
        List<IChunkPartition> far = new List<IChunkPartition>();
        foreach (List<IChunkPartition> partitionList in partitions) {
            foreach (IChunkPartition partition in partitionList) {
                if (partition.getLoaded() && !partition.inRange(target,Global.ChunkPartitionLoadRange.x,Global.ChunkPartitionLoadRange.y)) {
                    far.Add(partition);
                } 
            }
        }
        return far;
    }

    public Vector2Int getPosition()
    {
        return this.position;
    }

    public int getDim()
    {
        return this.dim;
    }

    public Transform getEntityContainer()
    {
        return entityContainer;
    }

    public Transform getTileEntityContainer()
    {
        return tileEntityContainer;
    }

    public bool partionsAreAllUnloaded()
    {
        foreach (List<IChunkPartition> partitionList in partitions) {
            foreach (IChunkPartition partition in partitionList) {
                if (partition.getLoaded() || partition.getScheduledForUnloading()) {
                    return false;
                }
            }
        }
        return true;
    }
}
