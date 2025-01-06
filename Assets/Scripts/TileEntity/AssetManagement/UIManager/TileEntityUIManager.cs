using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TileEntity {
    [System.Serializable]
    public class TileEntityUIManager
    {
        private bool loading;
        public bool Loading => loading;
        public bool Loaded => uiElementPrefab != null;
        public AssetReference AssetReference;
        private GameObject uiElementPrefab;
        public void loadUIIntoMemory() {
            if (!AssetReference.RuntimeKeyIsValid()) {
                return;
            }
            this.loading = true;
            AsyncOperationHandle<GameObject> handle = AssetReference.LoadAssetAsync<GameObject>();
            handle.Completed += onLoad;
        }
        public void onLoad(AsyncOperationHandle<GameObject> handle) {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                uiElementPrefab = handle.Result;
            }
            else
            {
                Debug.LogError("Failed to load asset: " + handle.Status);
            }
            loading = false;
            Addressables.Release(handle);
        }
        public void unloadUIFromMemory() {
            Addressables.Release(uiElementPrefab);
            uiElementPrefab = null;
        }
        public GameObject getUIElement() {
            return uiElementPrefab;
        }
        public void display<TInstance, TUIElement>(TInstance tileEntityInstance) where TInstance : ITileEntityInstance where TUIElement : ITileEntityUI<TInstance>{
            if (AssetReference == null) {
                Debug.LogError($"Cannot display ui for {tileEntityInstance.getName()}: No asset reference");
                return;
            }
            if (uiElementPrefab == null) {
                Debug.LogError($"Cannot display ui for {tileEntityInstance.getName()}: UI prefab not loaded into memory");
                return;
            }
            GameObject instantiated = GameObject.Instantiate(uiElementPrefab);
            TUIElement uiComponent = instantiated.GetComponent<TUIElement>();
            if (uiComponent == null) {
                Debug.LogError($"Cannot display ui for {tileEntityInstance.getName()}: Prefab doesn't have component {typeof(TUIElement).Name}");
                return;
            }
            uiComponent.DisplayTileEntityInstance(tileEntityInstance);
            CanvasController.Instance.DisplayObject(instantiated.gameObject);
        }
    }
}

