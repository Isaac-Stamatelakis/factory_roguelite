using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TileEntity {
    public class TileEntityAssetManager : AddressableAssetManager
    {
        public TileEntityAssetManager(Dictionary<string, AssetReference> keyReferenceDict) {
            dict = new Dictionary<string, object>();
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
    }
}

