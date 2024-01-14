using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DimController : MonoBehaviour
{
    protected List<ClosedChunkSystem> closedChunkSystems;
    private Transform playerTransform;
    private int playerXChunk;
    private int previousPlayerXChunk;
    private int previousPlayerYChunk;
    private int playerYChunk;   

    public virtual void Awake() {
        closedChunkSystems = new List<ClosedChunkSystem>();
        playerTransform = GameObject.Find("Player").transform;
    }

    // Update is called once per frame
    public virtual void Update()
    {
        previousPlayerXChunk = playerXChunk;
        previousPlayerYChunk = playerYChunk;
        playerXChunk = (int) Mathf.Floor(playerTransform.position.x / 8f);
        playerYChunk = (int) Mathf.Floor(playerTransform.position.y / 8f);
        if (previousPlayerXChunk != playerXChunk || previousPlayerYChunk != playerYChunk) {
            foreach (ClosedChunkSystem closedChunkSystem in closedChunkSystems) {
                closedChunkSystem.playerChunkUpdate();
            }
        }
    }
}
