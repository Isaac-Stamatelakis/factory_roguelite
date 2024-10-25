using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AddressableUtils
{
    public static IEnumerator loadAsyncType<T>(AssetReference assetReference, string key, Dictionary<string,object> data) {
        var handle = assetReference.LoadAssetAsync<Object>();
        yield return handle;
        if (handle.Status == AsyncOperationStatus.Succeeded) {
            if (handle.Result is T typeVal) {
                data[key] = typeVal;
            } else {
                Debug.LogWarning($"{handle.Result.name} is not {typeof(T).Name}");
            }
        } else {
            Debug.LogWarning($"Failed to load {assetReference}");
        }
    }

    public static IEnumerator loadAsyncTypeList<T>(List<AssetReference> assetReferences, string key, Dictionary<string,object> data) {
        List<T> list = new List<T>();
        List<AsyncOperationHandle<Object>> handles = new List<AsyncOperationHandle<Object>>();
        foreach (AssetReference assetReference in assetReferences) {
            handles.Add(assetReference.LoadAssetAsync<Object>());
        }
        
        for (int i = 0; i < assetReferences.Count; i++) {
            var handle = handles[i];
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded) {
                if (handle.Result is T typeVal) {
                    list.Add(typeVal);
                } else {
                    Debug.LogWarning($"{handle.Result.name} is not {typeof(T).Name}");
                }
            } else {
                Debug.LogWarning($"Failed to load {assetReferences[i]}");
            }
        }
        data[key] = list;
    }

    public static T validateHandle<T>(AsyncOperationHandle<Object> handle) {
        if (handle.Status == AsyncOperationStatus.Succeeded) {
            if (handle.Result is T val) {
                return val;
            } else {
                Debug.LogWarning($"{handle.Result.name} is not {typeof(T).Name}");
            }
        } else {
            Debug.LogWarning($"Failed to load {handle.OperationException}");
        }
        return default(T);
    }

    public static List<T> validateHandles<T>(List<AsyncOperationHandle<Object>> handles) {
        List<T> values = new List<T>();
        foreach (var handle in handles) {
            T value = validateHandle<T>(handle);
            if (value == null) {
                continue;
            }
            values.Add(value);
        }
        return values;
    }
}
