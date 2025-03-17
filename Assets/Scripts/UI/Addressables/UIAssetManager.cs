using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UI {
    [System.Serializable]
    public class UIAssetManager : AddressableAssetManager {
        [SerializeField] private List<UIAssetKey> assetKeys;
        public void load() {
            dict = new Dictionary<string, object>();
            foreach (var UIAssetKey in assetKeys) {
                AssetReference assetReference = UIAssetKey.AssetReference;
                string key = UIAssetKey.Key;
                if (assetReference == null) {
                    continue;
                }
                AsyncOperationHandle<Object> handle = assetReference.LoadAssetAsync<Object>();
                handle.Completed += (handle) => onLoad(handle, key);
            }
        }

        public void DisplayObject(string key)
        {
            GameObject uiObject = cloneGameObject(key);
            CanvasController.Instance.DisplayObject(uiObject);
        }
        
        public void DisplayObject(string key, int priority, bool terminateOnEscape)
        {
            GameObject uiObject = cloneGameObject(key);
            CanvasController.Instance.DisplayObject(uiObject,priority:priority,terminateOnEscape:terminateOnEscape);
        }
    }

    
}

