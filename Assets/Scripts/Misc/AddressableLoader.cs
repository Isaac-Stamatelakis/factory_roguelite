using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
public static class AddressableLoader
{
    /// <summary>
    /// Loads a prefab from a path and gets component T
    /// </summary>
    public static async Task<T> getPrefabComponent<T>(string path) where T : Component {
        var handle = Addressables.LoadAssetAsync<GameObject>(path);
        GameObject instance = await handle.Task; // Await the loading
        if (instance != null) {
            GameObject clone = GameObject.Instantiate(instance);
            return clone.GetComponent<T>();
        }
        Debug.LogError("Failed to load prefab at path: " + path);
        return default(T);
    }

    public static T getPrefabComponentInstantly<T>(string path) where T : Component {
        var handle = Addressables.LoadAssetAsync<GameObject>(path);
        GameObject instance = handle.WaitForCompletion(); // Await the loading
        if (instance != null) {
            GameObject clone = GameObject.Instantiate(instance);
            return clone.GetComponent<T>();
        }
        Debug.LogError("Failed to load prefab at path: " + path);
        return default(T);
    }

    /// <summary>
    /// Loads a prefab from a path
    /// </summary>
    public static async Task<GameObject> getPrefab(string path) {
        var handle = Addressables.LoadAssetAsync<GameObject>(path);
        GameObject instance = await handle.Task; // Await the loading
        if (instance != null) {
            GameObject clone = GameObject.Instantiate(instance);
            return clone;
        }
        Debug.LogError("Failed to load prefab at path: " + path);
        return default(GameObject);
    }

    /// <summary>
    /// Loads a prefab from a path
    /// </summary>
    public static GameObject getPrefabInstantly(string path) {
        var handle = Addressables.LoadAssetAsync<GameObject>(path);
        GameObject instance = handle.WaitForCompletion();
        if (instance != null) {
            GameObject clone = GameObject.Instantiate(instance);
            return clone;
        }
        Debug.LogError("Failed to load prefab at path: " + path);
        return default(GameObject);
    }
}
