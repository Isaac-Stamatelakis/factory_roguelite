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
        [SerializeField] public TextMeshProUGUI mStatusText;
        [SerializeField] private AssetReference mEditUIPrefabRef;
        private CompactMachineInstance compactMachine;
        private CompactMachineMetaData metaData;

        private CompactMachineEditUI editUIPrefab;
        
        const string BLUE_PRINT_HEADER = "Instances: ";
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
            metaData = CompactMachineUtils.GetMetaDataFromHash(compactMachine.Hash);
            mDepthText.text = $"Depth: {key.Path.Count-1}";
            mSubSystemText.text = $"Sub-Systems: {compactMachine.GetSubSystems()}";
            
            mIdText.text = $"ID: '{tileEntityInstance.Hash}'";
            mStatusText.text = GetStatusText();
            
            if (metaData != null)
            {
                DisplayMetaData(metaData);
                mNameTextField.onValueChanged.AddListener((value) =>
                {
                    metaData.Name = value;
                });

                void OnStatusChange()
                {
                    mStatusText.text = GetStatusText();
                }

                void OnHashChange()
                {
                    mIdText.text = $"ID: '{tileEntityInstance.Hash}'";
                    DisplayMetaData(CompactMachineUtils.GetMetaDataFromHash(compactMachine.Hash));
                }
                mEditButton.onClick.AddListener(() =>
                {
                    CompactMachineEditUI editUI = Instantiate(editUIPrefab);
                    editUI.Display(metaData, compactMachine, OnStatusChange, OnHashChange);
                    CanvasController.Instance.DisplayObject(editUI.gameObject);
                });
                
            }
            else
            {
                mBluePrintInstancesText.text = $"{BLUE_PRINT_HEADER}?";
                mEditButton.interactable = false;
                mDetailedViewButton.interactable = false;
            }
            
            mTeleportButton.onClick.AddListener(() =>
            {
                CompactMachineUtils.TeleportIntoCompactMachine(tileEntityInstance);
            });
        }

        private void DisplayMetaData(CompactMachineMetaData metaData)
        {
            mNameTextField.text = metaData.Name;
            mBluePrintInstancesText.text = $"{BLUE_PRINT_HEADER}{metaData.Instances+1}";
            
        }

        private string GetStatusText()
        {
            string statusText = "Status: ";
            if (!compactMachine.IsActive)
            {
                return statusText + "De-Activated";
            }
            
            if ((metaData != null && metaData.Locked) || compactMachine.IsParentLocked())
            {
                return statusText + "Locked";
            }

            return statusText + "Activate";
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

