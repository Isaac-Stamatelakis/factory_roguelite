using System;
using System.Collections.Generic;
using Item.Slot;
using Items;
using Items.Inventory;
using Player;
using Player.Tool;
using PlayerModule;
using PlayerModule.Mouse;
using Robot.Upgrades;
using Robot.Upgrades.Info;
using Robot.Upgrades.LoadOut;
using Robot.Upgrades.Network;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Robot.Tool.UI
{
    public class PlayerToolListUI : MonoBehaviour
    {
        [SerializeField] private PlayerToolUIIndicator primaryIndicator;
        [SerializeField] private PlayerToolUIIndicator secondaryIndicator;
        private Image image;
        [SerializeField] private Color highlightColor;
        private Color defaultColor;
        private PlayerInventory playerInventory;
        [SerializeField] private InventoryUI mToolCollectionUI;
        [SerializeField] public RobotUpgradeStatSelectorUI robotUpgradeStatSelectorUIPrefab;
        private PlayerScript playerScript;
        public void UpdateIndicators()
        {
            IRobotToolInstance current = playerInventory.CurrentTool;
            primaryIndicator.Display(current);   
        }
        public void Start()
        {
            image = GetComponent<Image>();
            defaultColor = image.color;
        }

        public void Initialize(List<IRobotToolInstance> tools, PlayerScript playerScript)
        {
            this.playerScript = playerScript;
            this.playerInventory = playerScript.PlayerInventory;
            List<ItemSlot> toolItemSlots = new List<ItemSlot>();
            foreach (IRobotToolInstance robotToolInstance in tools)
            {
                toolItemSlots.Add(new ItemSlot(robotToolInstance.GetToolObject().ToolIconItem,1,null));
                Debug.Log(robotToolInstance.GetName());
                var itemObject = robotToolInstance.GetToolObject().ToolIconItem;
                if (!itemObject) continue;
                Debug.Log(itemObject.name);
                //Debug.Log(itemObject?.GetSprite()?.name);
            }
     
            mToolCollectionUI.DisplayInventory(toolItemSlots,clear:false);
            mToolCollectionUI.InventoryInteractMode = InventoryInteractMode.OverrideAction;

            void SelectTool(PointerEventData.InputButton inputButton, int index)
            {
                switch (inputButton)
                {
                    case PointerEventData.InputButton.Left:
                        playerInventory.ChangeSelectedTool(index);
                        DisplayIndicators(index);
                        break;
                    case PointerEventData.InputButton.Right:
                        OpenToolLoadOut(index);
                        break;
                    case PointerEventData.InputButton.Middle:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(inputButton), inputButton, null);
                }
            }

            string ToolTipOverride(int index)
            {
                RobotToolType robotToolType = playerScript.PlayerRobot.ToolTypes[index];
                string message = GlobalHelper.AddSpaces(robotToolType.ToString());
                int loadOut = playerScript.PlayerRobot.RobotUpgradeLoadOut.ToolLoadOuts[robotToolType].Current;
                message += $"\nCurrent LoadOut {RobotUpgradeUtils.FormatLoadOut(loadOut)}";
                //message += $"\nLeft Click to Select";
                //message += $"\nRight Click to Modify LoadOut";
                return message;

            }
            mToolCollectionUI.SetToolTipOverride(ToolTipOverride);
        
            void DisplayIndicators(int index)
            {
                RefreshIndicator(tools[index]);
                int yPosition = 12 - index * 80;
                RectTransform toolIndicatorContainer = (RectTransform)primaryIndicator.transform.parent;
                Vector3 localPosition = toolIndicatorContainer.anchoredPosition;
                localPosition.y = yPosition;
                toolIndicatorContainer.anchoredPosition = localPosition;
            }
            mToolCollectionUI.OverrideClickAction(SelectTool);
            List<string> topTexts = new List<string>();
            for (int i = 0; i < toolItemSlots.Count; i++)
            {
                topTexts.Add((i+1).ToString());
            }
            mToolCollectionUI.DisplayTopText(topTexts);
        
            for (int i = 0; i < toolItemSlots.Count; i++)
            {
                var type =  playerScript.PlayerRobot.RobotData.ToolData.Types[i];
                int loadOut = playerScript.PlayerRobot.RobotUpgradeLoadOut.ToolLoadOuts[type].Current;
                SetLoadOutText(i, loadOut);
            }
        

            void LockSlot(ItemSlotUI slotUI)
            {
                slotUI.LockBottomText = true;
                slotUI.LockTopText = true;
            }
            mToolCollectionUI.ApplyFunctionToAllSlots(LockSlot);
            
            mToolCollectionUI.SetAllPanelColors(new Color(125/255f,202/255f,1f,1f));
            mToolCollectionUI.SetHighlightColor(new Color(222/255f,218/255f,91/255f,1f));
            mToolCollectionUI.SetOnHighlight(DisplayIndicators);
        
            SelectTool(PointerEventData.InputButton.Left,0);
        }
    
        public void RefreshIndicator(IRobotToolInstance current) 
        {
            primaryIndicator.Display(current);
            bool secondaryActive = current is ISubModeRobotToolInstance;
            if (secondaryActive) secondaryIndicator.Display(current);
            secondaryIndicator.gameObject.SetActive(secondaryActive);
        }
    
        public void SetLoadOutText(int index, int loadOut)
        {
            ItemSlotUI itemSlotUI = mToolCollectionUI.GetItemSlotUI(index);
            itemSlotUI.LockBottomText = false;
            itemSlotUI.DisplayBottomText(RobotUpgradeUtils.FormatLoadOut(loadOut));
            itemSlotUI.LockBottomText = true;
        }
        
        public void Highlight(bool state)
        {
            image.color = state ? highlightColor : defaultColor;
        }

        public void HighLightTool(int index)
        {
            mToolCollectionUI.HighlightSlot(index);
        }

        public void OpenToolLoadOut(int index)
        {
            PlayerRobot playerRobot = playerScript.PlayerRobot;
            string upgradePath = playerRobot.RobotTools[index].GetToolObject().UpgradePath;
            List<RobotUpgradeData> upgradeData = playerRobot.RobotData.ToolData.Upgrades[index];
            RobotToolType toolType = playerRobot.ToolTypes[index];
            IRobotToolInstance robotToolInstance = playerRobot.RobotTools[index];
            RobotStatLoadOutCollection statLoadOutCollection = playerRobot.RobotUpgradeLoadOut.GetToolLoadOut(toolType);
            RobotUpgradeInfo robotUpgradeInfo = RobotUpgradeInfoFactory.GetRobotUpgradeInfo(RobotUpgradeType.Tool,(int)toolType);
                    
            void OnLoadOutChange(int loadOut)
            {
                SetLoadOutText(index,loadOut);
            }

            string title = $"{robotToolInstance.GetToolObject().name} Upgrades";

            RobotUpgradeStatSelectorUI.UpgradeDisplayData upgradeDisplayData = new RobotUpgradeStatSelectorUI.UpgradeDisplayData(
                upgradePath, statLoadOutCollection, upgradeData, robotUpgradeInfo, OnLoadOutChange,null,title);
            RobotUpgradeStatSelectorUI statSelectorUI = GameObject.Instantiate(robotUpgradeStatSelectorUIPrefab);
            bool success = statSelectorUI.Display(upgradeDisplayData);
            if (success) CanvasController.Instance.DisplayObject(statSelectorUI.gameObject);
        }
    }
}
