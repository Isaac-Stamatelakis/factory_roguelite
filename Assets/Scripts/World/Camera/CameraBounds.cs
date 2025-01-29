using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks.Systems;


public class CameraBounds : MonoBehaviour
{
    private FloatIntervalVector bounds;
    private ClosedChunkSystem closedChunkSystem;
    private float yOffset;

    public void Awake()
    {
        yOffset = transform.localPosition.y;
    }
    public void SetSystem(ClosedChunkSystem closedChunkSystem, bool bound) {
        this.closedChunkSystem = closedChunkSystem;
        if (!bound) {
            bounds = null;
            return;
        }
        IntervalVector intervalVector = closedChunkSystem.GetBounds();
        int worldChunkSize = Global.CHUNK_SIZE/2; // Chunks size is in tile size which is 1/2 world size
        this.bounds = new FloatIntervalVector(
            new Interval<float>(
                intervalVector.X.LowerBound,
                intervalVector.X.UpperBound+worldChunkSize
            ),
            new Interval<float>(
                intervalVector.Y.LowerBound,
                intervalVector.Y.UpperBound+worldChunkSize
            )
            
        );
    }
    private float height;
    private float width;
    private Vector2Int lastPartition = new Vector2Int(int.MinValue,int.MinValue);
    private Vector2Int lastChunk = new Vector2Int(int.MinValue,int.MinValue);

    public void SetSize() {
        Vector3[] frustumCorners = new Vector3[4];
        Camera camera = GetComponent<Camera>();
        camera.CalculateFrustumCorners(
            new Rect(0, 0, 1, 1), // Use full screen for corners calculation
            camera.transform.position.y, // Distance to calculate corners from
            Camera.MonoOrStereoscopicEye.Mono, // Use Mono for non-stereoscopic cameras
            frustumCorners); // Output array for corners

        // Determine the area covered by the camera
        height = 2f * camera.orthographicSize;
        width = height * camera.aspect;
        if (!ReferenceEquals(closedChunkSystem,null)) {
            closedChunkSystem.PlayerChunkUpdate();
            closedChunkSystem.PlayerPartitionUpdate();
        }
    }

    private void CheckPartitionAndChunk()
    {
        if (ReferenceEquals(closedChunkSystem, null)) return;
        
        Vector3 position = transform.position;
        int worldPartitionSize = Global.CHUNK_PARTITION_SIZE >> 1;
        int px = (int) position.x / worldPartitionSize % Global.PARTITIONS_PER_CHUNK;
        int py = (int) position.y / worldPartitionSize % Global.PARTITIONS_PER_CHUNK;
        if (px == lastPartition.x && py == lastPartition.y) {
            return;
        }
        closedChunkSystem.PlayerPartitionUpdate();
        lastPartition = new Vector2Int(px,py);
        
        int cx = (int) position.x / (Global.PARTITIONS_PER_CHUNK/2);
        int cy = (int) position.y / (Global.PARTITIONS_PER_CHUNK/2);
        
        if (cx == lastChunk.x && cy == lastChunk.y) {
            return;
        }
        
        closedChunkSystem.PlayerChunkUpdate();
        lastChunk = new Vector2Int(cx,cy);
    }

    public void UpdateCameraBounds()
    {
        if (bounds == null) return;
        Transform playerTransform = transform.parent;
        Vector3 position = transform.localPosition;
        bool outLeft = playerTransform.position.x-width/2 < bounds.X.LowerBound;
        bool outRight = playerTransform.position.x + width/2 > bounds.X.UpperBound;
        if (outLeft) {
            position.x = bounds.X.LowerBound+width/2-playerTransform.position.x;
        } else if (outRight) {
            position.x = bounds.X.UpperBound-width/2-playerTransform.position.x;
        } else {
            position.x = 0;
        }
        bool outBottom = playerTransform.position.y - height/2 - yOffset < bounds.Y.LowerBound;
        bool outTop = playerTransform.position.y + height/2 + yOffset > bounds.Y.UpperBound;
        if (outTop) {
            position.y = bounds.Y.UpperBound-height/2 - playerTransform.position.y - yOffset;
        } else if (outBottom) {
            position.y = bounds.Y.LowerBound+height/2 - playerTransform.position.y + yOffset;
        } else {
            position.y = 0;
        }

        position += Vector3.up * yOffset;
        transform.localPosition = position;
    }

    public void Update() {
        CheckPartitionAndChunk();
    }
}


