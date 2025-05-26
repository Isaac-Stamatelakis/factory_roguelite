using System;
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
    [SerializeField] private Camera uiCamera;
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
        SetViewRange(viewSize);
    }
    
    public void SetViewRange(CameraViewSize cameraViewSize)
    {
        int height = Screen.height;
        int width = Screen.width;
        float scale = GetCameraSize(cameraViewSize);
        int cameraWidth = Mathf.RoundToInt(width * scale);
        int cameraHeight = Mathf.RoundToInt(height * scale);
        PixelPerfectCamera pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
        pixelPerfectCamera.enabled = true;
        pixelPerfectCamera.refResolutionX = cameraWidth;
        pixelPerfectCamera.refResolutionY = cameraHeight;
        const int PIXELS_PER_TILE = 16;
        
        Vector2 partitionsPerScreen = new Vector2(cameraWidth,cameraHeight) / (PIXELS_PER_TILE * Global.CHUNK_PARTITION_SIZE * 2);
        const int BONUS_RANGE = 3;
        chunkPartitionLoadRange = new Vector2Int((int)partitionsPerScreen.x, (int)partitionsPerScreen.y)+BONUS_RANGE*Vector2Int.one;
        
        Debug.Log($"Camera size set '{cameraWidth} by {cameraHeight}' pixels, and partition load range '{chunkPartitionLoadRange}'");
        StartCoroutine(SetCameraSizeDelayed());
    }

    private float GetCameraSize(CameraViewSize cameraViewSize)
    {
        switch (cameraViewSize)
        {
            case CameraViewSize.Small:
                return 1/4f;
            case CameraViewSize.Medium:
                return 1/3f;
            case CameraViewSize.Large:
                return 1/2f;
            default:
                return 1 / 2f;
        }
    }
    
    private IEnumerator SetCameraSizeDelayed() {
        yield return new WaitForEndOfFrame();
        GetComponent<CameraBounds>().SetSize();
        Camera selfCamera = GetComponent<Camera>();
        uiCamera.orthographicSize = selfCamera.orthographicSize;
    }

    public void SetDebugRange(float cameraSize) {
        PixelPerfectCamera pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
        pixelPerfectCamera.enabled = false;
        GetComponent<Camera>().orthographicSize = cameraSize;
    }
}
