using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Dimensions;
using TileEntity.Instances.CompactMachine;
using TileEntity.Instances.CompactMachine.UI;
using UI;
using UnityEngine.AddressableAssets;

namespace TileEntity.Instances.CompactMachines {
    public class CompactMachineUIController : MonoBehaviour, ITileEntityUI<CompactMachineInstance>
    {
        [SerializeField] public TextMeshProUGUI mTitle;
        [SerializeField] public TextMeshProUGUI mIdText;
        [SerializeField] public TextMeshProUGUI mPositionText;
        [SerializeField] public TextMeshProUGUI mSubSystemText;
        [SerializeField] public TextMeshProUGUI mDepthText;
        [SerializeField] public TextMeshProUGUI mBluePrintInstancesText;
        [SerializeField] public Button mDetailedViewButton;
        [SerializeField] public Button mTeleportButton;
        [SerializeField] public Button mEditButton;
        [SerializeField] public TMP_InputField mNameTextField;
        [SerializeField] private AssetReference mEditUIPrefabRef;
        private CompactMachineInstance compactMachine;
        private CompactMachineMetaData metaData;

        private CompactMachineEditUI editUIPrefab;
        public void Start()
        {
            StartCoroutine(LoadAssets());
        }

        private IEnumerator LoadAssets()
        {
            var editHandle = Addressables.LoadAssetAsync<GameObject>(mEditUIPrefabRef);
            yield return editHandle;
            editUIPrefab = editHandle.Result.GetComponent<CompactMachineEditUI>();
            Addressables.Release(editHandle);
        }

        public void DisplayTileEntityInstance(CompactMachineInstance tileEntityInstance)
        {
            if (tileEntityInstance.Hash == null)
            {
                Debug.LogWarning("Cannot display compact machine with null hash");
                return;
            }
            this.compactMachine = tileEntityInstance;
            mTitle.text = tileEntityInstance.TileEntityObject.name;
            CompactMachineTeleportKey key = tileEntityInstance.GetTeleportKey();
            mPositionText.text = GetPositionText(key);
            string path = Path.Combine(CompactMachineUtils.GetCompactMachineHashFoldersPath(), compactMachine.Hash);
            metaData = CompactMachineUtils.GetMetaData(path);
            mSubSystemText.text = $"Sub-Systems: {compactMachine.GetSubSystems()}";
            mDepthText.text = $"Depth: {key.Path.Count-1}";
            mIdText.text = $"ID: '{tileEntityInstance.Hash}'";
            const string BLUE_PRINT_HEADER = "Instances";
            
            if (metaData != null)
            {
                mNameTextField.text = metaData.Name;
                mNameTextField.onValueChanged.AddListener((value) =>
                {
                    metaData.Name = value;
                });
                mBluePrintInstancesText.text = $"{BLUE_PRINT_HEADER}: {metaData.Instances+1}";
                mEditButton.onClick.AddListener(() =>
                {
                    CompactMachineEditUI editUI = Instantiate(editUIPrefab);
                    editUI.Display(metaData,compactMachine);
                    CanvasController.Instance.DisplayObject(editUI.gameObject);
                });
            }
            else
            {
                mBluePrintInstancesText.text = $"{BLUE_PRINT_HEADER}: ?";
                mEditButton.interactable = false;
                mDetailedViewButton.interactable = false;
            }
            
            mTeleportButton.onClick.AddListener(() =>
            {
                CompactMachineUtils.TeleportIntoCompactMachine(tileEntityInstance);
            });
        }
        
        private string GetPositionText(CompactMachineTeleportKey key)
        {
            string text = "Position: ";
            for (var index = 0; index < key.Path.Count; index++)
            {
                var vector2Int = key.Path[index];
                text += $"[{vector2Int.x},{vector2Int.y}]";
                if (index < key.Path.Count-1) text += "->";
            }

            return text;
        }

        public void OnDestroy()
        {
            string path = Path.Combine(CompactMachineUtils.GetCompactMachineHashFoldersPath(), compactMachine.Hash);
            CompactMachineUtils.SaveMetaDataJson(metaData,path);
        }
    }
}

