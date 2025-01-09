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
    public void setSystem(ClosedChunkSystem closedChunkSystem, bool bound) {
        this.closedChunkSystem = closedChunkSystem;
        if (!bound) {
            bounds = null;
            return;
        }
        IntervalVector intervalVector = closedChunkSystem.getBounds();
        int worldChunkSize = Global.ChunkSize/2; // Chunks size is in tile size which is 1/2 world size
        this.bounds = new FloatIntervalVector(
            new Interval<float>(
                intervalVector.X.LowerBound,
                intervalVector.X.UpperBound+worldChunkSize
            ),
            new Interval<float>(
                intervalVector.Y.LowerBound-worldChunkSize/2,
                intervalVector.Y.UpperBound+worldChunkSize/2
            )
        );
    }
    private float height;
    private float width;
    private Vector2Int lastPartition = new Vector2Int(int.MinValue,int.MinValue);
    private Vector2Int lastChunk = new Vector2Int(int.MinValue,int.MinValue);

    public void setSize() {
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
        if (closedChunkSystem != null) {
            closedChunkSystem.playerChunkUpdate();
            closedChunkSystem.playerPartitionUpdate();
        }
    }

    private void checkPartitionAndChunk() {
        if (closedChunkSystem == null) {
            return;
        }
        Vector3 position = transform.position;
        int worldPartitionSize = Global.ChunkPartitionSize >> 1;
        int px = (int) position.x / worldPartitionSize % Global.PartitionsPerChunk;
        int py = (int) position.y / worldPartitionSize % Global.PartitionsPerChunk;
        if (px == lastPartition.x && py == lastPartition.y) {
            return;
        }
        closedChunkSystem.playerPartitionUpdate();
        lastPartition = new Vector2Int(px,py);

        int worldChunkSize = Global.PartitionsPerChunk >> 1;
        int cx = (int) position.x / (Global.PartitionsPerChunk/2);
        int cy = (int) position.y / (Global.PartitionsPerChunk/2);
        
        if (cx == lastChunk.x && cy == lastChunk.y) {
            return;
        }
        
        closedChunkSystem.playerChunkUpdate();
        lastChunk = new Vector2Int(cx,cy);
    }

    public void Update() {
        if (bounds == null) {
            checkPartitionAndChunk();
            return;
        }
        // TODO CHANGE THIS SO ITS ONLY CALLED WHEN THE PLAYER MOVES
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
        bool outBottom = playerTransform.position.y - height/2 < bounds.Y.LowerBound;
        bool outTop = playerTransform.position.y + height/2 > bounds.Y.UpperBound;
        if (outTop) {
            position.y = bounds.Y.UpperBound-height/2 - playerTransform.position.y;
        } else if (outBottom) {
            position.y = bounds.Y.LowerBound+height/2 - playerTransform.position.y;
        } else {
            position.y = 0;
        }
        position += new Vector3(0,yOffset,0);
        transform.localPosition = position;

        checkPartitionAndChunk();
    }
}


