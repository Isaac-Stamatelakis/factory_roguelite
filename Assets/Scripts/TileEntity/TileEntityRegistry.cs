using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace TileEntityModule {
    public enum LoadType {
        Hard,
        Soft
    }
    public class TileEntityRegistry
    {
        private static TileEntityRegistry instance;
        private Dictionary<string, TileEntity> idDict;
        private TileEntityRegistry() {
            idDict = new Dictionary<string, TileEntity>();
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
            TileEntity tileEntity = idDict[tileItem.id];
            switch (loadType) {
                case LoadType.Hard:
                    break;
                case LoadType.Soft:
                    if (tileEntity.SoftLoadable) {
                        return;
                    }
                    break;
            }
            Addressables.Release(tileEntity);
        }
        private void OnAssetLoaded(AsyncOperationHandle<TileEntity> handle, string id) {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                TileEntity loadedTileEntity = handle.Result;
                idDict[id] = loadedTileEntity;
            }
            else
            {
                Debug.LogError("Failed to load asset: " + handle.Status);
            }

        }
        
    }

}
