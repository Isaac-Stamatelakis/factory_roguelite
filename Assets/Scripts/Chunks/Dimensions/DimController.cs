using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DimController : MonoBehaviour
{
    protected List<ClosedChunkSystem> closedChunkSystems;
    private Transform playerTransform;
    private int playerXChunk;
    private int playerYChunk;   

    private int previousPlayerXChunk;
    private int previousPlayerYChunk;
    private int playerXPartition;
    private int playerYPartition;
    private int previousPlayerXPartition;
    private int previousPlayerYPartition;
    
    public virtual void Start() {
        closedChunkSystems = new List<ClosedChunkSystem>();
        playerTransform = GameObject.Find("Player").transform;
    }

    // Update is called once per frame
    public virtual void Update()
    {
        previousPlayerXChunk = playerXChunk;
        previousPlayerYChunk = playerYChunk;
        playerXChunk = (int) Mathf.Floor(playerTransform.position.x / (Global.PartitionsPerChunk/2));
        playerYChunk = (int) Mathf.Floor(playerTransform.position.y / (Global.PartitionsPerChunk/2));
        if (previousPlayerXChunk != playerXChunk || previousPlayerYChunk != playerYChunk) {
            foreach (ClosedChunkSystem closedChunkSystem in closedChunkSystems) {
                closedChunkSystem.playerChunkUpdate();
            }
        }

        previousPlayerXPartition = playerXPartition;
        previousPlayerYPartition = playerYPartition;
        playerXPartition = (int) Mathf.Floor(playerTransform.position.x) / (Global.ChunkPartitionSize/2) % Global.PartitionsPerChunk;
        playerYPartition = (int) Mathf.Floor(playerTransform.position.y) / (Global.ChunkPartitionSize/2) % Global.PartitionsPerChunk;
        if (previousPlayerXPartition != playerXPartition || previousPlayerYPartition != playerYPartition) {
            foreach (ClosedChunkSystem closedChunkSystem in closedChunkSystems) {
                closedChunkSystem.playerPartitionUpdate();
            }
        }
    }
}
