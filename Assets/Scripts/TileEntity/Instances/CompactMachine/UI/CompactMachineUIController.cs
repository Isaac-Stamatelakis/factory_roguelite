using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Dimensions;
using TileEntity.Instances.CompactMachine;

namespace TileEntity.Instances.CompactMachines {
    public class CompactMachineUIController : MonoBehaviour, ITileEntityUI<CompactMachineInstance>
    {
        [SerializeField] public TextMeshProUGUI mTitle;
        [SerializeField] public TextMeshProUGUI mIdText;
        [SerializeField] public TextMeshProUGUI mPositionText;
        [SerializeField] public TextMeshProUGUI mSubSystemText;
        [SerializeField] public TextMeshProUGUI mDepthText;
        [SerializeField] public Button mDetailedViewButton;
        [SerializeField] public Button mTeleportButton;
        [SerializeField] public Button mLockButton;
        [SerializeField] public TextMeshProUGUI mLockText;
        [SerializeField] public Button mActivateButton;
        [SerializeField] public TextMeshProUGUI mActivateText;
        [SerializeField] public TMP_InputField mNameTextField;
        private CompactMachineInstance compactMachine;
        private CompactMachineMetaData metaData;
        
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
            if (metaData != null)
            {
                mNameTextField.text = metaData.Name;
                mNameTextField.onValueChanged.AddListener((value) =>
                {
                    metaData.Name = value;
                });
                SetLockText(metaData.Locked);
                mLockButton.onClick.AddListener(() =>
                {
                    metaData.Locked = !metaData.Locked;
                    SetLockText(metaData.Locked);
                });
            }
            
            mTeleportButton.onClick.AddListener(() =>
            {
                CompactMachineUtils.TeleportIntoCompactMachine(tileEntityInstance);
            });
        }

        private void SetLockText(bool locked)
        {
            mLockText.text = locked ? "Locked" : "Unlocked";
        }

        private void SetActivateText(bool activated)
        {
            mActivateText.text = activated ? "Activated" : "Unactivated";
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

