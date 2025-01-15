using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public enum CameraViewSize {
    Small,
    Medium,
    Large,
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
        pixelPerfectCamera.enabled = true;
        switch (cameraViewSize) {
            case CameraViewSize.Small:
                pixelPerfectCamera.refResolutionX = 480;
                pixelPerfectCamera.refResolutionY = 270;
                chunkPartitionLoadRange = new Vector2Int(5,4);
                break;
            case CameraViewSize.Medium:
                chunkPartitionLoadRange = new UnityEngine.Vector2Int(6,5);
                pixelPerfectCamera.refResolutionX = 640;
                pixelPerfectCamera.refResolutionY = 360;
                break;
            case CameraViewSize.Large:
                chunkPartitionLoadRange = new Vector2Int(8,6);
                pixelPerfectCamera.refResolutionX = 960;
                pixelPerfectCamera.refResolutionY = 540;
                break;    
        }
        StartCoroutine(setCameraSizeDelayed());
    }

    private IEnumerator setCameraSizeDelayed() {
        yield return new WaitForEndOfFrame();
        GetComponent<CameraBounds>().SetSize();
    }

    public void setDebugRange(float cameraSize) {
        PixelPerfectCamera pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
        pixelPerfectCamera.enabled = false;
        GetComponent<Camera>().orthographicSize = cameraSize;
    }
}
