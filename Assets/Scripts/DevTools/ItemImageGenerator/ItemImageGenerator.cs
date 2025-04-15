using System;
using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using Items;
using UnityEngine;
using UnityEngine.UI;

using System.IO;
using Item.Display;


namespace DevTools.ItemImageGenerator
{
    public class ItemImageGenerator : MonoBehaviour
    {
        private const string SAVE_PATH = "C:\\Users\\Isaac\\Documents\\Coding\\unity\\CaveTechImages";
        private const int RESOLUTION = 64;
        [SerializeField] private Camera mCaptureCamera;
        
        private int maxCaptures = 100;
        public bool forceCapture;


        public void Awake()
        {
            mCaptureCamera = Camera.main;
        }
        
        public IEnumerator CaptureCoroutine(List<ItemObject> itemObjects, Action onComplete)
        {
            mCaptureCamera.clearFlags = CameraClearFlags.SolidColor;
            Color originalColor = mCaptureCamera.backgroundColor;
            mCaptureCamera.backgroundColor = Color.clear;
            
            int width = mCaptureCamera.pixelWidth;
            int height = mCaptureCamera.pixelHeight;

            
            string folder = Path.Combine(SAVE_PATH, "Images");
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
            Directory.CreateDirectory(folder);
            
            float worldHeight = mCaptureCamera.orthographicSize;
            float worldWidth = worldHeight * mCaptureCamera.aspect;
            int rows = (int) (2*worldWidth / Global.TILE_SIZE);
            int cols = (int) (2*worldHeight / Global.TILE_SIZE);
           
            int captures = (itemObjects.Count-1)/(rows*cols)+1;
            for (int capture = 0; capture < captures; capture++)
            {
                GameObject container = new GameObject("ItemContainer");
                container.transform.position = new Vector3(-worldWidth, -worldHeight, 0) + Vector3.one * Global.TILE_SIZE/2f;
            
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        int idx = r + c*rows + capture * captures;
                        if (idx >= itemObjects.Count) break;
                        ItemObject itemObject = itemObjects[idx];
                        Display(container.transform,r,c,itemObject);
                    }
                }

                yield return new WaitForFixedUpdate();
                RenderTexture renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
                mCaptureCamera.targetTexture = renderTexture;
            
                mCaptureCamera.Render();
                RenderTexture.active = renderTexture;
                
                Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
                tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                tex.Apply();

                mCaptureCamera.targetTexture = null;
                RenderTexture.active = null;
                
                int imageSize = 64;
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        int idx = r + c*rows + capture * captures;
                        if (idx >= itemObjects.Count) break;
                        ItemObject itemObject = itemObjects[idx];
                        Color[] pixels = tex.GetPixels(
                            r * imageSize, 
                            c * imageSize, 
                            imageSize, 
                            imageSize
                        );
                        
                        Texture2D chunkTex = new Texture2D(imageSize, imageSize, TextureFormat.RGBA32, false);
                        chunkTex.SetPixels(pixels);
                        chunkTex.Apply();
                        
                        byte[] bytes = chunkTex.EncodeToPNG();
                        string path = Path.Combine(folder, $"{itemObject?.id}.png");
                        File.WriteAllBytes(path, bytes);
                        
                        UnityEngine.Object.Destroy(chunkTex);
                    }
                }

                Destroy(container);
                Destroy(tex);
                renderTexture.Release();
            }
            Debug.Log($"Generated {itemObjects.Count} Images at {folder}");
            mCaptureCamera.backgroundColor = originalColor;
            onComplete.Invoke();
            GameObject.Destroy(gameObject);
        }
        

        public void Display(Transform container, int row, int col, ItemObject itemObject)
        {
            GameObject itemEntity = new GameObject($"Item[{row},{col}]");
            ItemWorldDisplay itemWorldDisplay = itemEntity.AddComponent<ItemWorldDisplay>();
            ItemSlot itemSlot = new ItemSlot(itemObject, 1, null);
            itemWorldDisplay.Display(itemSlot);
            
            itemEntity.transform.SetParent(container,false);
            itemEntity.transform.localPosition = new Vector3(Global.TILE_SIZE*row, Global.TILE_SIZE*col, 0);

            SpriteRenderer spriteRenderer = itemEntity.GetComponent<SpriteRenderer>();
            List<Sprite> sprites = new List<Sprite>();
            if (spriteRenderer?.sprite)
            {
                sprites.Add(spriteRenderer.sprite);
            }
            SpriteRenderer[] additionalRenderers = itemEntity.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer additionalRenderer in additionalRenderers)
            {
                if (additionalRenderer.sprite)
                {
                    sprites.Add(additionalRenderer.sprite);
                }
            }

            Vector2 largestSpriteSize = Vector2.zero;
            foreach (Sprite sprite in sprites)
            {
                Vector2 spriteSize = sprite.bounds.size;
                if (spriteSize.x > largestSpriteSize.x)
                {
                    largestSpriteSize.x = spriteSize.x;
                }

                if (spriteSize.y > largestSpriteSize.y)
                {
                    largestSpriteSize.y = spriteSize.y;
                }
            }
            if (largestSpriteSize == Vector2.zero) return;

            float GetScale(float size)
            {
                if (size <= 1) return 0.5f;
                return 1/(2*size);
            }

            float xScale = GetScale(largestSpriteSize.x);
            float yScale = GetScale(largestSpriteSize.y);
            if (xScale < yScale) yScale = xScale;
            if (yScale < xScale) xScale = yScale;
            itemEntity.transform.localScale = new Vector3(xScale, yScale, itemEntity.transform.localScale.z);
        }
    }
}
