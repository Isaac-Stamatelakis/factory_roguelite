using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TileEntityModule {
    public class TileEntityAssetManager
    {
        private Dictionary<string, object> assetDict;
        public TileEntityAssetManager(Dictionary<string, AssetReference> keyReferenceDict) {
            assetDict = new Dictionary<string, object>();
            foreach (var kvp in keyReferenceDict) {
                AssetReference assetReference = kvp.Value;
                string key = kvp.Key;
                if (assetReference == null) {
                    continue;
                }
                AsyncOperationHandle<Object> handle = assetReference.LoadAssetAsync<Object>();
                handle.Completed += (handle) => onLoad(handle, key);
            }
        }
        public void onLoad(AsyncOperationHandle<Object> handle, string key) {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var element = handle.Result;
                if (assetDict.ContainsKey(element.name)) {
                    Debug.LogWarning($"TileEntityAssetManager loaded asset with duplicate name: {element.name}");
                }
                assetDict[element.name] = element;
            }
            else
            {
                Debug.LogError("Failed to load asset: " + handle.Status);
            }
            Addressables.Release(handle);
        }
        public void unloadUIFromMemory() {
            
        }

        public T getElement<T>(string key) where T : Object {
            if (assetDict.ContainsKey(key)) {
                var value = assetDict[key];
                if (value is T element) {
                    return element;
                }
                return null;
            }
            return null;
        }

        public C cloneElement<C>(string key) where C : Component {
            GameObject element = cloneGameObject(key);
            if (element == null) {
                return null;
            }
            return element.GetComponent<C>();
        }

        public GameObject cloneGameObject(string key) {
            GameObject element = getElement<GameObject>(key);
            if (element == null) {
                return null;
            }
            return GameObject.Instantiate(element);
        }
    }
}

