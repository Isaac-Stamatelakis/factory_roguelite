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
        
        internal void Display(CompactMachineMetaData metaData, CompactMachineInstance compactMachine)
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
            }
            else
            {
                mStatusText.text = "Editable";
                mStatusReasonText.gameObject.SetActive(false);
                SetLockText(this.metaData.Locked);
                mLockButton.onClick.AddListener(() =>
                {
                    this.metaData.Locked = !this.metaData.Locked;
                    SetLockText(this.metaData.Locked);
                });
                mActivateButton.onClick.AddListener(() =>
                {
                    compactMachine.SetActive(!compactMachine.IsActive);
                    SetActiveText(compactMachine.IsActive);
                });
            }
            
        }

        private void SetLockText(bool locked)
        {
            mLockButton.GetComponentInChildren<TextMeshProUGUI>().text = locked ? "Locked" : "Unlocked";
        }
        
        private void SetActiveText(bool locked)
        {
            mLockButton.GetComponentInChildren<TextMeshProUGUI>().text = locked ? "Locked" : "Unlocked";
        }
    }
}
