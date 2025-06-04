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
using UI.Indicators.General;
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
        
        void Start()
        {
            playerInventory = GetComponent<PlayerInventory>();
            playerScript = GetComponent<PlayerScript>();
            
            var miscKeys = playerScript.InputActions.MiscKeys;
            ControlUtils.AssignAction(miscKeys.ItemSearch,PlayerControl.OpenSearch,OpenSearch);
            ControlUtils.AssignAction(miscKeys.Inventory,PlayerControl.OpenInventory, _ => playerInventory.ToggleInventoryMode());
            ControlUtils.AssignAction(miscKeys.ConduitOptions,PlayerControl.OpenConduitOptions, _ => playerScript.TileViewers.ConduitViewController.DisplayView());
            ControlUtils.AssignAction(miscKeys.ConduitPortView,PlayerControl.SwitchConduitPortView, _ => ChangePortModePress());
            ControlUtils.AssignAction(miscKeys.SwitchPlacementMode,PlayerControl.SwitchPlacementMode, SwitchPlacementMode);
            ControlUtils.AssignAction(miscKeys.SubPlacementMode,PlayerControl.SwitchPlacementSubMode, SwitchSubPlacementMode);
            ControlUtils.AssignAction(miscKeys.TerminateConduitGroup,PlayerControl.TerminateConduitGroup,TerminateConduitGroup);
            ControlUtils.AssignAction(miscKeys.ConduitView,PlayerControl.ChangeConduitViewMode,_ => playerScript.PlayerUIContainer.TileIndicatorManagerUI.conduitPlacementModeIndicatorUI.DisplayLoadOutEditor());
            ControlUtils.AssignAction(miscKeys.PlacePreview,PlayerControl.PlacePreview, _ => playerScript.PlayerUIContainer.TileIndicatorManagerUI.tileHighligherIndicatorUI.Toggle());
            ControlUtils.AssignAction(miscKeys.SmartPlacement,PlayerControl.SmartPlace, _ => playerScript.PlayerUIContainer.TileIndicatorManagerUI.tileSearchIndicatorUI.Toggle());
            
            miscKeys.InteractTools.performed += _ => playerInventory.SetInteractMode(InteractMode.Tools);
            miscKeys.InteractTools.canceled += _ => playerInventory.SetInteractMode(InteractMode.Inventory);
            miscKeys.ShowChat.performed += _ => TextChatUI.Instance.ShowTextField();
            miscKeys.Enable();

            
            var toolBindings = playerScript.InputActions.ToolBindings;
            ControlUtils.AssignAction(toolBindings.AutoSelect,PlayerControl.AutoSelect, AutoSelect);
            ControlUtils.AssignAction(toolBindings.SwitchToolMode,PlayerControl.SwitchToolMode, SwitchToolMode);
            ControlUtils.AssignAction(toolBindings.OpenRobotLoadout,PlayerControl.OpenRobotLoadOut, OpenRobotLoadOut);
            ControlUtils.AssignAction(toolBindings.OpenToolLoadout,PlayerControl.OpenToolLoadOut, OpenToolLoadOut);
            ControlUtils.AssignAction(toolBindings.SwitchRobotLoadout,PlayerControl.SwapRobotLoadOut, SwitchRobotLoadout);
            ControlUtils.AssignAction(toolBindings.SwitchToolLoadout,PlayerControl.SwapToolLoadOut, SwitchToolLoadOut);
            toolBindings.Enable();
            
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
            inventoryNavigation.Scroll.performed += ScrollInventory;
            inventoryNavigation.Enable();
            
            var inventoryUtils = playerScript.InputActions.InventoryUtils;
            ControlUtils.AssignAction(inventoryUtils.ShowRecipes,PlayerControl.ShowItemRecipes, ShowItemRecipes);
            ControlUtils.AssignAction(inventoryUtils.ShowUses,PlayerControl.ShowItemUses, ShowItemUses);
            ControlUtils.AssignAction(inventoryUtils.EditTag,PlayerControl.EditItemTag, EditItemTags);
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


        void ScrollInventory(InputAction.CallbackContext context)
        {
            float y = UnityEngine.InputSystem.Mouse.current.scroll.ReadValue().y;
            if (y < 0) {
                playerInventory.IterateSelectedTile(1);
            } else if (y > 0) {
                playerInventory.IterateSelectedTile(-1);
            }
        }
        void OpenSearch(InputAction.CallbackContext context)
        {
            ItemSearchUI itemSearchUI = Instantiate(playerScript.Prefabs.ItemSearchUIPrefab);
            itemSearchUI.Initialize(playerScript);
            CanvasController.Instance.DisplayObject(itemSearchUI.gameObject,terminatorContextPath:new ContextPathWrapper(ref context),blocker:false,blockMovement:false);
        }

        void SwitchPlacementMode(InputAction.CallbackContext context)
        {
            TileStateIndicatorUI tileStateIndicatorUI = playerScript.PlayerUIContainer.TileIndicatorManagerUI.tileStateIndicatorUI;
            tileStateIndicatorUI.Toggle(1);
        }
        
        void SwitchSubPlacementMode(InputAction.CallbackContext context)
        {
            TileRotationIndicatorUI rotationIndicatorUI = playerScript.PlayerUIContainer.TileIndicatorManagerUI.rotationIndicatorUI;
            rotationIndicatorUI.Toggle(1);
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
            playerScript.PlayerUIContainer.IndicatorManager.autoSelectIndicator.Iterate(1);
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