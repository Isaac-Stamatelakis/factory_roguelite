using System;
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
using UnityEngine.InputSystem;
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

            var miscKeys = playerScript.InputActions.MiscKeys;
            miscKeys.ItemSearch.performed += OpenSearch;
            miscKeys.Inventory.performed += _ => playerInventory.ToggleInventoryMode();
            miscKeys.PlacementMode.performed += SwitchPlacementMode;
            miscKeys.ConduitOptions.performed += _ => playerScript.TileViewers.ConduitViewController.DisplayRadialMenu();
            miscKeys.ConduitPortView.performed += _ => ChangePortModePress();
            miscKeys.InteractTools.performed += _ => playerInventory.SetInteractMode(InteractMode.Tools);
            miscKeys.InteractTools.canceled += _ => playerInventory.SetInteractMode(InteractMode.Inventory);
            miscKeys.ShowChat.performed += _ => TextChatUI.Instance.ShowTextField();
            
            var toolBindings = playerScript.InputActions.ToolBindings;
            toolBindings.AutoSelect.performed += AutoSelect;
            toolBindings.SwitchMode.performed += SwitchToolMode;
            toolBindings.OpenLoadout.performed += OpenRobotLoadOut;
            toolBindings.SwitchLoadout.performed += SwitchRobotLoadout;
            
            
            var inventoryNavigation = playerScript.InputActions.InventoryNavigation;
            inventoryNavigation.Select1.performed += _ => playerInventory.ChangeSelectedSlot(0);
            inventoryNavigation.Select2.performed += _ => playerInventory.ChangeSelectedSlot(1);
            inventoryNavigation.Select3.performed += _ => playerInventory.ChangeSelectedSlot(2);
            inventoryNavigation.Select4.performed += _ => playerInventory.ChangeSelectedSlot(3);
            inventoryNavigation.Select5.performed += _ => playerInventory.ChangeSelectedSlot(4);
            inventoryNavigation.Select6.performed += _ => playerInventory.ChangeSelectedSlot(5);
            inventoryNavigation.Select7.performed += _ => playerInventory.ChangeSelectedSlot(6);
            inventoryNavigation.Select8.performed += _ => playerInventory.ChangeSelectedSlot(7);
            inventoryNavigation.Select9.performed += _ => playerInventory.ChangeSelectedSlot(8);
            inventoryNavigation.Select0.performed += _ => playerInventory.ChangeSelectedSlot(9);

            var inventoryUtils = playerScript.InputActions.InventoryUtils;
            inventoryUtils.ShowRecipes.performed += ShowItemRecipes;
            inventoryUtils.ShowUses.performed += ShowItemUses;
            inventoryUtils.EditTag.performed += EditItemTags;
            
            toolBindings.Enable();
            miscKeys.Enable();
            inventoryNavigation.Enable();
            inventoryUtils.Enable();
        }

        public void SyncEventsWithUIMode(bool active)
        {
            SetState(playerScript.InputActions.MiscKeys.Get());
            SetState(playerScript.InputActions.InventoryNavigation.Get());
            SetState(playerScript.InputActions.ToolBindings);
            
            void SetState(InputActionMap inputActionMap)
            {
                if (active)
                {
                    inputActionMap.Disable();
                }
                else
                {
                    inputActionMap.Enable();
                }
            }
        }
        

        void OpenSearch(InputAction.CallbackContext context)
        {
            ItemSearchUI itemSearchUI = Instantiate(playerScript.Prefabs.ItemSearchUIPrefab);
            itemSearchUI.Initialize(playerScript);
            CanvasController.Instance.DisplayObject(itemSearchUI.gameObject, keyCodes: ControlUtils.GetKeyCodes(PlayerControl.OpenSearch),blocker:false,blockMovement:false);
        }

        void SwitchPlacementMode(InputAction.CallbackContext context)
        {
            ConduitPlacementOptions conduitPlacementOptions = playerScript.ConduitPlacementOptions;
            conduitPlacementOptions.ResetPlacementRecord();
            conduitPlacementOptions.PlacementMode = GlobalHelper.ShiftEnum(1, conduitPlacementOptions.PlacementMode);
            playerScript.PlayerUIContainer.IndicatorManager.conduitPlacementModeIndicatorUI.Display();
        }

        void TerminateConduitGroup(InputAction.CallbackContext context)
        {
            ConduitPlacementOptions conduitPlacementOptions = playerScript.ConduitPlacementOptions;
            conduitPlacementOptions.ResetPlacementRecord();
        }

        void SwitchToolMode(InputAction.CallbackContext context)
        {
            IRobotToolInstance current = playerInventory?.CurrentTool;
            if (current != null)
            {
                Robot.Tool.MoveDirection moveDirection = Keyboard.current.shiftKey.wasPressedThisFrame ? Robot.Tool.MoveDirection.Right : Robot.Tool.MoveDirection.Left;
                current.ModeSwitch(moveDirection,Keyboard.current.ctrlKey.wasPressedThisFrame);
                playerScript.PlayerMouse.UpdateOnToolChange();
                playerScript.PlayerInventory.PlayerRobotToolUI.UpdateIndicators();
            }
        }

        void SwitchRobotLoadout(InputAction.CallbackContext context)
        {
            var selfLoadout = playerScript.PlayerRobot.RobotUpgradeLoadOut.SelfLoadOuts;
            selfLoadout.IncrementCurrent(1);
            playerScript.PlayerUIContainer.IndicatorManager.loadOutIndicator.SetLoadOut(selfLoadout.Current);
        }

        void SwitchToolLoadOut(InputAction.CallbackContext context)
        {
            var loadOut = playerScript.PlayerRobot.RobotUpgradeLoadOut.ToolLoadOuts[playerScript.PlayerInventory.CurrentToolType];
            loadOut.IncrementCurrent(1);
            playerScript.PlayerInventory.PlayerRobotToolUI.SetLoadOutText(playerScript.PlayerInventory.CurrentToolIndex,loadOut.Current);
        }

        void AutoSelect(InputAction.CallbackContext context)
        {
            bool autoSelect = playerScript.PlayerMouse.ToggleAutoSelect();
            playerScript.PlayerUIContainer.IndicatorManager.autoSelectIndicator.Display(autoSelect);
        }

        void OpenRobotLoadOut(InputAction.CallbackContext context)
        {
            playerScript.PlayerUIContainer.IndicatorManager.loadOutIndicator.OpenRobotLoadOut();
        }

        void OpenToolLoadOut(InputAction.CallbackContext context)
        {
            playerScript.PlayerInventory.PlayerRobotToolUI.OpenToolLoadOut(playerScript.PlayerInventory.CurrentToolIndex);
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
        

        void ShowItemRecipes(InputAction.CallbackContext context)
        {
            ItemSlotUIClickHandler clickHandler = GetPointerOverComponent<ItemSlotUIClickHandler>();
            clickHandler?.ShowRecipes();
        }

        void ShowItemUses(InputAction.CallbackContext context)
        {
            ItemSlotUIClickHandler clickHandler = GetPointerOverComponent<ItemSlotUIClickHandler>();
            clickHandler?.ShowUses();
        }

        void EditItemTags(InputAction.CallbackContext context)
        {
            ItemSlotUIClickHandler clickHandler = GetPointerOverComponent<ItemSlotUIClickHandler>();
            if (clickHandler is ITagEditableItemSlotUI)
            {
                ItemSlotTagEditor.EditItemTag(clickHandler.GetInventoryItem());
            }
        }

        public static T GetPointerOverComponent<T>() where T : Component
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = UnityEngine.InputSystem.Mouse.current.position.ReadValue()
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
    }
}