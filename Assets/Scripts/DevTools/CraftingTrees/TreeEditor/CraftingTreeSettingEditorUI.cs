using System;
using DevTools.CraftingTrees.Network;
using Recipe.Objects;
using TileEntity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DevTools.CraftingTrees.TreeEditor
{
    
    internal class CraftingTreeSettingEditorUI : MonoBehaviour
    {
        [SerializeField] private Button mItemButton;
        [SerializeField] private Button mTransmutationButton;
        [SerializeField] private Button mProcessorButton;
        [SerializeField] private Color highlightColor;
        [SerializeField] private TMP_Dropdown mTierDropDown;
        [SerializeField] private TMP_Dropdown mTransmutationEfficiencyDropDown;
        [SerializeField] private TextMeshProUGUI mEnergyBalanceText;
        private CraftingTreeNodeType generateNodeType;
        private Button currentHighlightButton;
        private CraftingTreeGenerator craftingTreeGenerator;

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SwitchCraftingType(mItemButton, CraftingTreeNodeType.Item);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SwitchCraftingType(mTransmutationButton, CraftingTreeNodeType.Transmutation);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SwitchCraftingType(mProcessorButton, CraftingTreeNodeType.Processor);
            }
        }

        private void SwitchCraftingType(Button button, CraftingTreeNodeType type)
        {
            if (generateNodeType == type) return;
            generateNodeType = type;
            if (currentHighlightButton)
            {
                currentHighlightButton.GetComponent<Image>().color = button.GetComponent<Image>().color;
            }
            currentHighlightButton = button;
            currentHighlightButton.GetComponent<Image>().color = highlightColor;
            craftingTreeGenerator.SetType(type);
        }

        public void Initialize(CraftingTreeNodeNetwork craftingTreeNodeNetwork, CraftingTreeGenerator treeGenerator)
        {
            craftingTreeGenerator = treeGenerator;
            void InitializeNodeButton(Button button, CraftingTreeNodeType type)
            {
                if (generateNodeType == type)
                {
                    button.GetComponent<Image>().color = highlightColor;
                    currentHighlightButton = button;
                    treeGenerator.SetType(type);
                }
                button.onClick.AddListener(() =>
                {
                    SwitchCraftingType(button, type);
                });
            }

            InitializeNodeButton(mItemButton, CraftingTreeNodeType.Item);
            InitializeNodeButton(mTransmutationButton, CraftingTreeNodeType.Transmutation);
            InitializeNodeButton(mProcessorButton, CraftingTreeNodeType.Processor);
            mTierDropDown.options = GlobalHelper.EnumToDropDown<Tier>();
            mTransmutationEfficiencyDropDown.options = GlobalHelper.EnumToDropDown<TransmutationEfficency>();
        }
    }
}
