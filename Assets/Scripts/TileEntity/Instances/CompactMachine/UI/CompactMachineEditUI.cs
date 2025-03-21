using System;
using System.IO;
using Chunks;
using Chunks.Systems;
using Dimensions;
using TileEntity.Instances.CompactMachines;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TileEntity.Instances.CompactMachine.UI
{
    public class CompactMachineEditUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI mStatusText;
        [SerializeField] private TextMeshProUGUI mStatusReasonText;
        [SerializeField] private Button mLockButton;
        [SerializeField] private Button mActivateButton;

        private CompactMachineMetaData metaData;
        
        internal void Display(CompactMachineMetaData metaData, CompactMachineInstance compactMachine, Action onStatusChange, Action onHashChange)
        {
            
            this.metaData = metaData;
            bool parentLocked = compactMachine.IsParentLocked();
            if (parentLocked)
            {
                mLockButton.interactable = false;
                mActivateButton.interactable = false;
                mStatusText.text = "Not Editable";
                mStatusReasonText.text = "Reason: Parent Locked";
                SetLockText(true);
                SetActiveText(compactMachine.IsActive);
                return;
            }
            mStatusText.text = "Editable";
            mStatusReasonText.gameObject.SetActive(false);
            SetLockText(this.metaData.Locked);
            mLockButton.onClick.AddListener(() =>
            {
                this.metaData.Locked = !this.metaData.Locked;
                if (!this.metaData.Locked)
                {
                    OnUnLock(metaData, compactMachine, onHashChange);
                }
                else
                {
                    compactMachine.SaveSystem();
                    string hashPath = Path.Combine(CompactMachineUtils.GetCompactMachineHashFoldersPath(), compactMachine.Hash);
                    string dimPath = CompactMachineUtils.GetPositionFolderPath(compactMachine.GetTeleportKey().Path);
                    GlobalHelper.CopyDirectory(dimPath, hashPath);
                }
                SetLockText(this.metaData.Locked);
                onStatusChange.Invoke();
            });
            mActivateButton.onClick.AddListener(() =>
            {
                compactMachine.SetActive(!compactMachine.IsActive);
                SetActiveText(compactMachine.IsActive);
                onStatusChange.Invoke();
            });
            
        }

        private void OnUnLock(CompactMachineMetaData metaData, CompactMachineInstance compactMachine, Action onHashChange)
        {
            string hashPath = Path.Combine(CompactMachineUtils.GetCompactMachineHashFoldersPath(), compactMachine.Hash);
            if (!Directory.Exists(hashPath)) return;
            if (metaData.Instances <= 1) // If instances less than one no need to store content data at hash
            {
                string[] directories = Directory.GetDirectories(hashPath);
                foreach (string directory in directories)
                {
                    Directory.Delete(directory, true);
                }
                
                return;
            }
            Debug.Log("Instance reduced");
            // If a compact machine is blue printed and unlocked, must create a hash instance for it.
            this.metaData.Instances--;
            CompactMachineUtils.SaveMetaDataJson(this.metaData,hashPath);
            string newHash = CompactMachineUtils.GenerateHash();
            compactMachine.SetHash(newHash);
            CompactMachineUtils.InitializeHashFolder(newHash, compactMachine.getId());
            onHashChange.Invoke();

        }

        private void SetLockText(bool locked)
        {
            mLockButton.GetComponentInChildren<TextMeshProUGUI>().text = locked ? "Locked" : "Unlocked";
        }
        
        private void SetActiveText(bool active)
        {
            mActivateButton.GetComponentInChildren<TextMeshProUGUI>().text = active ? "Activated" : "De-Activated";
        }
    }
}
