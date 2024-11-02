using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public abstract class AddressableAssetManager
{
    protected Dictionary<string, object> dict;
    public bool hasAsset(string key) {
        return dict.ContainsKey(key);
    }
    public int Count => dict.Count;
    protected void onLoad(AsyncOperationHandle<Object> handle, string key) {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            var element = handle.Result;
            if (dict.ContainsKey(key)) {
                Debug.LogWarning($"AddressableAssetManager loaded asset with duplicate name: {key}");
            }
            dict[key] = element;
        }
        else
        {
            Debug.LogError("Failed to load asset: " + handle.Status);
        }
        Addressables.Release(handle);
    }
    
    public T getElement<T>(string key) where T : Object {
        if (dict.ContainsKey(key)) {
            var value = dict[key];
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

    [System.Serializable]
    protected class UIAssetKey {
        public string Key;
        public AssetReference AssetReference;
    }
    

    
}
