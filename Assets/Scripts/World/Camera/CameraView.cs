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
    private const int MAX_WIDTH = 960;
    private const int MAX_HEIGHT = 540;
    private const float HEIGHT_WIDTH_RATIO = 1080f / 1920f;
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

    public void FixedUpdate()
    {
        
    }

    private int GetCameraViewSizeWidth(CameraViewSize cameraViewSize)
    {
        switch (cameraViewSize)
        {
            case CameraViewSize.Small:
                return 480;
            case CameraViewSize.Medium:
                return 640;
            case CameraViewSize.Large:
                return 960;
            default:
                throw new ArgumentOutOfRangeException(nameof(cameraViewSize), cameraViewSize, null);
        }
    }
    
    public void SetViewRange(CameraViewSize cameraViewSize)
    {
        SetViewRange(GetCameraViewSizeWidth(cameraViewSize));
    }
    public void SetViewRange(int width)
    {
        CameraViewSize cameraViewSize = GetNearestCameraViewSize(width);
        int cameraWidth = GetCameraViewSizeWidth(cameraViewSize);
        int cameraHeight = (int)(cameraWidth * HEIGHT_WIDTH_RATIO);
        int pixelRatio = Screen.width / cameraWidth;
        float t0 = (float) Screen.width / pixelRatio;
        float ratio = cameraWidth / t0;
        
        PixelPerfectCamera pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
        pixelPerfectCamera.enabled = true;
        pixelPerfectCamera.refResolutionX = cameraWidth;
        pixelPerfectCamera.refResolutionY = cameraHeight;
        
        const int PIXELS_PER_TILE = 16;
        Vector2 partitionsPerScreen = new Vector2(width,width*HEIGHT_WIDTH_RATIO) / (PIXELS_PER_TILE * Global.CHUNK_PARTITION_SIZE * 2);
        chunkPartitionLoadRange = new Vector2Int((int)partitionsPerScreen.x, (int)partitionsPerScreen.y)+Vector2Int.one;
        Debug.Log(chunkPartitionLoadRange);
        transform.localScale = new Vector3(ratio, ratio, 1);
        StartCoroutine(setCameraSizeDelayed());
    }

    private CameraViewSize GetNearestCameraViewSize(int width)
    {
        
        foreach (CameraViewSize size in Enum.GetValues(typeof(CameraViewSize)))
        {
            int viewRange = GetCameraViewSizeWidth(size);
            if (width <= viewRange) return size;
        }

        return CameraViewSize.Large;
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
