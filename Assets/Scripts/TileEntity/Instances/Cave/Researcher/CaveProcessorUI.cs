using System;
using Items.Inventory;
using Items.Tags;
using TileEntity.Instances.Machines;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TileEntity.Instances
{
    public class CaveProcessorUI : MonoBehaviour, ITileEntityUI<CaveProcessorInstance>
    {
        [SerializeField] private InventoryUI mDriveInputUI;
        [SerializeField] private InventoryUI mDriveOutputUI;
        [SerializeField] private ArrowProgressController mProgressBar;
        [SerializeField] private Image mTextIndicator;
        [SerializeField] private TMP_InputField mTextInput;
        [SerializeField] private VerticalLayoutGroup mTextList;

        [SerializeField] private TextMeshProUGUI mTextPrefab;

        private const int INDICATOR_UPDATES_PER_TOGGLE = 15;
        private int toggleUpdates;
        
        private CaveProcessorInstance caveProcessorInstance;
        public void DisplayTileEntityInstance(CaveProcessorInstance tileEntityInstance)
        {
            caveProcessorInstance = tileEntityInstance;
            mDriveInputUI.DisplayInventory(tileEntityInstance.InputDrives);
            mDriveInputUI.AddTagRestriction(ItemTag.CaveData);
            
            mDriveOutputUI.DisplayInventory(tileEntityInstance.OutputDrives);
            mDriveOutputUI.SetInteractMode(InventoryInteractMode.BlockInput);
            
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) ) {
                EnterCommand(mTextInput.text);
                mTextInput.text = string.Empty;
            }
        }

        private void EnterCommand(string text)
        {
            TextMeshProUGUI textElement = GameObject.Instantiate(mTextPrefab, mTextList.transform, false);
            textElement.text = text;
        }

        public void FixedUpdate()
        {
            toggleUpdates++;
            if (toggleUpdates >= INDICATOR_UPDATES_PER_TOGGLE)
            {
                toggleUpdates = 0;
                mTextIndicator.gameObject.SetActive(!mTextIndicator.gameObject.activeInHierarchy);
            }
            float progress = caveProcessorInstance.CopyDriveProcess?.Progress ?? 0;
            mProgressBar.SetArrowProgress(progress);
        }
    }
}
