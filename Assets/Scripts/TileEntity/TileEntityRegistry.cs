using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace TileEntity {
    public enum LoadType {
        Hard,
        Soft
    }
    public class TileEntityRegistry
    {
        private static TileEntityRegistry instance;
        private Dictionary<string, TileEntityObject> idDict;
        private TileEntityRegistry() {
            idDict = new Dictionary<string, TileEntityObject>();
        }
        public static TileEntityRegistry getInstance() {
            if (instance == null) {
                instance = new TileEntityRegistry();
            }
            return instance;
        }

        

        public void unloadTileEntity(TileItem tileItem, LoadType loadType) {
            if (tileItem == null || !idDict.ContainsKey(tileItem.id)) {
                return;
            }
            TileEntityObject tileEntityObject = idDict[tileItem.id];
            switch (loadType) {
                case LoadType.Hard:
                    break;
                case LoadType.Soft:
                    if (tileEntityObject.SoftLoadable) {
                        return;
                    }
                    break;
            }
            Addressables.Release(tileEntityObject);
        }
        private void OnAssetLoaded(AsyncOperationHandle<TileEntityObject> handle, string id) {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                TileEntityObject loadedTileEntityObject = handle.Result;
                idDict[id] = loadedTileEntityObject;
            }
            else
            {
                Debug.LogError("Failed to load asset: " + handle.Status);
            }

        }
        
    }

}
