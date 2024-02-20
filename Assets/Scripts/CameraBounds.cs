using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule.ClosedChunkSystemModule;

public class CameraBounds : MonoBehaviour
{
    private ClosedChunkSystem closedChunkSystem;

    public ClosedChunkSystem ClosedChunkSystem { get => closedChunkSystem; set => closedChunkSystem = value; }
    private Camera camera;
    public void Start() {
        this.camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (closedChunkSystem == null) {
            return;
        }
        //Debug.Log(closedChunkSystem.worldPositionInBounds((Vector2) transform.position-getViewArea()/2));
    }

    private Vector2 getViewArea() {
        return new Vector2(camera.orthographicSize * Screen.width / Screen.height, camera.orthographicSize);
    }
}
