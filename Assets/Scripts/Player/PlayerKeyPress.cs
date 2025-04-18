using System.Collections;
using System.Collections.Generic;
using Conduits.PortViewer;
using Conduits.Systems;
using Item.Display.ClickHandlers;
using Item.Inventory;
using Item.Inventory.ClickHandlers.Instances;
using Item.Tags;
using UnityEngine;
using UnityEngine.EventSystems;
using Items.Inventory;
using Player;
using Player.Controls;
using Player.UI;
using PlayerModule.Mouse;
using Robot.Tool;
using TMPro;
using UI;
using UI.Catalogue.ItemSearch;
using UI.Chat;
using UI.QuestBook;
using UI.RingSelector;
using UI.ToolTip;
using Unity.VisualScripting;
using MoveDirection = UnityEngine.EventSystems.MoveDirection;


namespace PlayerModule.KeyPress {
    
    public class PlayerKeyPress : MonoBehaviour
    {
        [SerializeField] private UIRingSelector ringSelectorPrefab;
        private PlayerInventory playerInventory;

        private PlayerScript playerScript;

        private CanvasController canvasController;
        // Start is called before the first frame update
        void Start()
        {
            playerInventory = GetComponent<PlayerInventory>();
            playerScript = GetComponent<PlayerScript>();
            canvasController = CanvasController.Instance;
        }

        // Update is called once per frame
        void Update()
        {
            inventoryKeyPresses();
            
            if (canvasController.UIActive) return;
            ControlUtils.UpdateModifierCount();
            
            if (ControlUtils.GetControlKeyDown(PlayerControl.OpenSearch))
            {
                ItemSearchUI itemSearchUI = Instantiate(playerScript.Prefabs.ItemSearchUIPrefab);
                itemSearchUI.Initialize(playerScript);
                CanvasController.Instance.DisplayObject(itemSearchUI.gameObject, keyCodes: ControlUtils.GetKeyCodes(PlayerControl.OpenSearch),blocker:false,blockMovement:false);
            }

            if (ControlUtils.GetControlKeyDown(PlayerControl.SwitchPlacementMode))
            {
                ConduitPlacementOptions conduitPlacementOptions = playerScript.ConduitPlacementOptions;
                conduitPlacementOptions.ResetPlacementRecord();
                conduitPlacementOptions.PlacementMode = GlobalHelper.ShiftEnum(1, conduitPlacementOptions.PlacementMode);
                playerScript.PlayerUIContainer.IndicatorManager.conduitPlacementModeIndicatorUI.Display();
            }

            if (ControlUtils.GetControlKeyDown(PlayerControl.TerminateConduitGroup))
            {
                ConduitPlacementOptions conduitPlacementOptions = playerScript.ConduitPlacementOptions;
                conduitPlacementOptions.ResetPlacementRecord();
            }

            if (ControlUtils.GetControlKeyDown(PlayerControl.SwitchConduitPortView))
            {
                ChangePortModePress();
            }
            
            if (ControlUtils.GetControlKeyDown(PlayerControl.SwitchToolMode))
            {
                IRobotToolInstance current = playerInventory?.CurrentTool;
                if (current != null)
                {
                    Robot.Tool.MoveDirection moveDirection = Input.GetKey(KeyCode.LeftShift) ? Robot.Tool.MoveDirection.Right : Robot.Tool.MoveDirection.Left;
                    current.ModeSwitch(moveDirection,Input.GetKey(KeyCode.LeftControl));
                    playerScript.PlayerMouse.UpdateOnToolChange();
                    playerScript.PlayerInventory.PlayerRobotToolUI.UpdateIndicators();
                }
            }
            
            if (ControlUtils.GetControlKeyDown(PlayerControl.SwapRobotLoadOut))
            {
                var selfLoadout = playerScript.PlayerRobot.RobotUpgradeLoadOut.SelfLoadOuts;
                selfLoadout.IncrementCurrent(1);
                playerScript.PlayerUIContainer.IndicatorManager.loadOutIndicator.SetLoadOut(selfLoadout.Current);
            }
            
            if (ControlUtils.GetControlKeyDown(PlayerControl.SwapToolLoadOut))
            {
                var loadOut = playerScript.PlayerRobot.RobotUpgradeLoadOut.ToolLoadOuts[playerScript.PlayerInventory.CurrentToolType];
                loadOut.IncrementCurrent(1);
                playerScript.PlayerInventory.PlayerRobotToolUI.SetLoadOutText(playerScript.PlayerInventory.CurrentToolIndex,loadOut.Current);
            }
            
            if (ControlUtils.GetControlKeyDown(PlayerControl.AutoSelect))
            {
                bool autoSelect = playerScript.PlayerMouse.ToggleAutoSelect();
                playerScript.PlayerUIContainer.IndicatorManager.autoSelectIndicator.Display(autoSelect);
            }
            
            if (ControlUtils.GetControlKeyDown(PlayerControl.OpenRobotLoadOut))
            {
                playerScript.PlayerUIContainer.IndicatorManager.loadOutIndicator.OpenRobotLoadOut();
            }
            
            if (ControlUtils.GetControlKeyDown(PlayerControl.OpenToolLoadOut))
            {
                playerScript.PlayerInventory.PlayerRobotToolUI.OpenToolLoadOut(playerScript.PlayerInventory.CurrentToolIndex);
            }
            
            inventoryNavigationKeys();
        }
        
        public void ChangePortModePress()
        {
            UIRingSelector ringSelector = GameObject.Instantiate(ringSelectorPrefab);
            List<RingSelectorComponent> ringSelectorComponents = new List<RingSelectorComponent>();
                
            List<(Color,string,PortViewMode)> modeList = new List<(Color,string,PortViewMode)>
            {
                (Color.grey,"Off",PortViewMode.None),
                (Color.green,"Item",PortViewMode.Item),
                (Color.blue,"Fluid",PortViewMode.Fluid),
                (Color.red,"Signal",PortViewMode.Signal),
                (Color.yellow,"Energy",PortViewMode.Energy),
                (Color.magenta,"Matrix",PortViewMode.Matrix),
            };
            PortViewerController portViewerController = playerScript.TileViewers.ConduitPortViewer;
            foreach (var (color, elementName, portViewMode) in modeList)
            {
                ringSelectorComponents.Add(new RingSelectorComponent(color,null,elementName,() => portViewerController.ChangePortViewMode(portViewMode)));
            }

            CanvasController.Instance.PopStack();
            RingSelectorComponent defaultComponent = new RingSelectorComponent(Color.cyan, null, "Auto", () => portViewerController.ChangePortViewMode(PortViewMode.Auto));
            ringSelector.Display(ringSelectorComponents, defaultComponent);
            CanvasController.Instance.DisplayObject(ringSelector.gameObject);
        }

        private void inventoryNavigationKeys() {
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                playerInventory.ChangeSelectedSlot(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2)) {
                playerInventory.ChangeSelectedSlot(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3)) {
                playerInventory.ChangeSelectedSlot(2);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4)) {
                playerInventory.ChangeSelectedSlot(3);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5)) {
                playerInventory.ChangeSelectedSlot(4);
            }
            if (Input.GetKeyDown(KeyCode.Alpha6)) {
                playerInventory.ChangeSelectedSlot(5);
            }
            if (Input.GetKeyDown(KeyCode.Alpha7)) {
                playerInventory.ChangeSelectedSlot(6);
            }
            if (Input.GetKeyDown(KeyCode.Alpha8)) {
                playerInventory.ChangeSelectedSlot(7);
            }
            if (Input.GetKeyDown(KeyCode.Alpha9)) {
                playerInventory.ChangeSelectedSlot(8);
            }
            if (Input.GetKeyDown(KeyCode.Alpha0)) {
                playerInventory.ChangeSelectedSlot(9);
            }
        }

        private void inventoryKeyPresses()
        {
            if (!EventSystem.current.IsPointerOverGameObject()) return;
            
            if (Input.GetKeyDown(KeyCode.R)) {
                ItemSlotUIClickHandler clickHandler = GetPointerOverComponent<ItemSlotUIClickHandler>();
                clickHandler?.ShowRecipes();
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                ItemSlotUIClickHandler clickHandler = GetPointerOverComponent<ItemSlotUIClickHandler>();
                clickHandler?.ShowUses();
            }

            if (Input.GetKeyDown(KeyCode.Y))
            {
                ItemSlotUIClickHandler clickHandler = GetPointerOverComponent<ItemSlotUIClickHandler>();
                if (clickHandler is ITagEditableItemSlotUI)
                {
                    ItemSlotTagEditor.EditItemTag(clickHandler.GetInventoryItem());
                }
            }
        }

        public static T GetPointerOverComponent<T>() where T : Component
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, results);
            if (results.Count <= 0) return null;
            
            GameObject hoveredElement = results[0].gameObject;
            T component = hoveredElement.GetComponent<T>();
            if (!ReferenceEquals(component, null)) return component;
            component = hoveredElement.GetComponentInParent<T>();
            return component;
        }
        

        private void controls() {

        }
    }
}