using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public enum CameraViewSize {
    Small,
    Medium,
    Large
}
public class CameraView : MonoBehaviour
{
    private static CameraView instance;
    public static CameraView Instance => instance;
    public void Awake() {
        instance = this;
    }
    public CameraViewSize viewSize;
    private static Vector2Int chunkPartitionLoadRange;
    public static Vector2Int ChunkPartitionLoadRange => chunkPartitionLoadRange;
    // Start is called before the first frame update
    void Start()
    {
        setLoadRange(viewSize);
    }

    public void setLoadRange(CameraViewSize cameraViewSize) {
        PixelPerfectCamera pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
        switch (cameraViewSize) {
            case CameraViewSize.Small:
                pixelPerfectCamera.refResolutionX = 400;
                pixelPerfectCamera.refResolutionY = 100;
                chunkPartitionLoadRange = new Vector2Int(5,4);
                return;
            case CameraViewSize.Medium:
                chunkPartitionLoadRange = new UnityEngine.Vector2Int(6,5);
                pixelPerfectCamera.refResolutionX = 614;
                pixelPerfectCamera.refResolutionY = 200;
                return;
            case CameraViewSize.Large:
                chunkPartitionLoadRange = new Vector2Int(8,6);
                pixelPerfectCamera.refResolutionX = 614;
                pixelPerfectCamera.refResolutionY = 400;
                return;      
        }
    }

    public void setDebugRange(float cameraSize) {
        PixelPerfectCamera pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
        pixelPerfectCamera.enabled = false;
        GetComponent<Camera>().orthographicSize = cameraSize;
    }
}
