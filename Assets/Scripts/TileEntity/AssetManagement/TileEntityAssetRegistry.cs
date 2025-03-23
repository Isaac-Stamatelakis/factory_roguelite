using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlayerModule;
using TileEntity.Instances.Machine.UI;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TileEntity.AssetManagement
{
    public struct TileEntityAssetData
    {
        public AssetReference AssetReference;
        public ScriptableObject ScriptableObject;
    }

    public enum TileEntityAssetType
    {
        UI,
        Misc
    }
    public class TileEntityAssetRegistry : MonoBehaviour
    {
        private static TileEntityAssetRegistry instance;
        public static TileEntityAssetRegistry Instance => instance;
        private Dictionary<string, GameObject> uiPrefabs;
        private Dictionary<string, GameObject> assetPrefabs;
        private HashSet<string> loadingUIPrefabs;
        private HashSet<string> loadingAssetPrefabs;
        public void Awake()
        {
            instance = this;
            if (uiPrefabs != null && assetPrefabs != null) return;
            uiPrefabs = new Dictionary<string, GameObject>();
            assetPrefabs = new Dictionary<string, GameObject>();
            loadingUIPrefabs = new HashSet<string>();
            loadingAssetPrefabs = new HashSet<string>();
        }

        public void LoadAssetsCoroutine(HashSet<TileEntityObject> tileEntityObjects)
        {
            StartCoroutine(LoadAssets(tileEntityObjects));
        }
        public IEnumerator LoadAssets(HashSet<TileEntityObject> tileEntityObjects)
        {
            List<TileEntityAssetData> uiAssetData = GetUIAssetData(tileEntityObjects, TileEntityAssetType.UI);
            List<TileEntityAssetData> miscAssetData = GetUIAssetData(tileEntityObjects, TileEntityAssetType.Misc);
            var uiAssetLoad = StartCoroutine(LoadAssets(uiAssetData, uiPrefabs));
            var miscAssetLoad = StartCoroutine(LoadAssets(miscAssetData, assetPrefabs));
            yield return uiAssetLoad;
            yield return miscAssetLoad;
            Debug.Log($"TileEntityAssetRegistry loaded {uiPrefabs.Count} UIPrefabs & {miscAssetData.Count} Misc Assets");
        }

        public void DisplayUI(ITileEntityInstance tileEntityInstance)
        {
            string id = GetId(tileEntityInstance.GetTileEntity());
            if (id == null) return;
            if (!uiPrefabs.TryGetValue(id, out GameObject prefab))
            {
                Debug.LogWarning($"Ui element of tile entity '{tileEntityInstance.GetTileEntity().name}' is not in uiPrefab dict");
                return;
            }
            MainCanvasController mainCanvasController = MainCanvasController.TInstance;
            GameObject uiElement = Instantiate(prefab);
            ITileEntityUI tileEntityUI = uiElement.GetComponent<ITileEntityUI>();
            if (tileEntityUI == null)
            {
                Debug.LogWarning($"Ui element of tile entity '{tileEntityInstance.GetTileEntity().name}' does not implement ITileEntityUI");
                return;
            }
            tileEntityUI.DisplayTileEntityInstance(tileEntityInstance);
            if (tileEntityUI is IInventoryUITileEntityUI or IInventoryUIAggregator)
            {
                mainCanvasController.DisplayUIWithPlayerInventory(uiElement);
            }
            else
            {
                mainCanvasController.DisplayObject(uiElement);
            }
            
        }
        private List<TileEntityAssetData> GetUIAssetData(HashSet<TileEntityObject> tileEntityObjects, TileEntityAssetType type)
        {
            List<TileEntityAssetData> tileEntityAssetDataList = new List<TileEntityAssetData>();
            foreach (TileEntityObject tileEntityObject in tileEntityObjects)
            {
                AssetReference assetReference = null;
                switch (type)
                {
                    case TileEntityAssetType.UI:
                        if (tileEntityObject is not IUITileEntity uiTileEntity) break;
                        assetReference = uiTileEntity.GetUIAssetReference();
                        break;
                    case TileEntityAssetType.Misc:
                        if (tileEntityObject is not IAssetTileEntity assetTileEntity) break;
                        assetReference = assetTileEntity.GetAssetReference();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }

                if (assetReference == null || !assetReference.RuntimeKeyIsValid()) continue;
                TileEntityAssetData tileEntityAssetData = new TileEntityAssetData
                {
                    AssetReference = assetReference,
                    ScriptableObject = tileEntityObject,
                };
                tileEntityAssetDataList.Add(tileEntityAssetData);
            }
            return tileEntityAssetDataList;
        }

        public void LoadAsset(TileEntityAssetType type, TileEntityObject tileEntityObject, AssetReference assetReference)
        {
            var (dict, loading) = GetDataStructures(type);
            
            StartCoroutine(LoadAssetCoroutine(type, tileEntityObject, assetReference,dict,loading));
        }

        private (Dictionary<string, GameObject>, HashSet<string>) GetDataStructures(TileEntityAssetType type)
        {
            switch (type)
            {
                case TileEntityAssetType.UI:
                    return (uiPrefabs, loadingUIPrefabs);
                case TileEntityAssetType.Misc:
                    return (assetPrefabs, loadingAssetPrefabs);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        private IEnumerator LoadAssetCoroutine(TileEntityAssetType type, TileEntityObject tileEntityObject, AssetReference assetReference, Dictionary<string, GameObject> dict, HashSet<string> loadingHandles)
        {
            if (!assetReference.RuntimeKeyIsValid())
            {
                Debug.LogWarning($"TileEntityAssetRegistry could not load asset of type '{type}' from tile entity '{tileEntityObject.name}'");
                yield break;
            }
            string id = GetId(tileEntityObject);
            if (dict.ContainsKey(id) || !loadingHandles.Add(id)) yield break;
            var handle = Addressables.LoadAssetAsync<GameObject>(assetReference);
            
            yield return handle;

            dict[id] = handle.Result;
            loadingHandles.Remove(id);
            
            Addressables.Release(handle);
            
        }
        

        public IEnumerator LoadAssets(List<TileEntityAssetData> uiTileEntities, Dictionary<string, GameObject> assetDict)
        {
            List<string> keys = assetDict.Keys.ToList();
            List<string> ids = new List<string>();
            List<string> newIds = new List<string>();
            List<AsyncOperationHandle<GameObject>> handles = new List<AsyncOperationHandle<GameObject>>();
            foreach (TileEntityAssetData tileEntityAssetData in uiTileEntities)
            {
                string id = GetId(tileEntityAssetData.ScriptableObject);
                if (id == null) continue;
                ids.Add(id);
                if (keys.Contains(id)) continue;
                var handle = Addressables.LoadAssetAsync<GameObject>(tileEntityAssetData.AssetReference);
                newIds.Add(id);
                handles.Add(handle);
            }

            for (var index = 0; index < handles.Count; index++)
            {
                var handle = handles[index];
                yield return handle;
                assetDict[newIds[index]] = handle.Result;
                Addressables.Release(handle);
            }

            foreach (string key in keys)
            {
                if (ids.Contains(key)) continue;
                assetDict.Remove(key);
            }
        }

        private static string GetId(ScriptableObject scriptableObject)
        {
            return !scriptableObject ? null : scriptableObject.name.ToLower().Replace(" ", "_");
        }
    }
}
