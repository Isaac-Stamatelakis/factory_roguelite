using System.Collections;
using System.Collections.Generic;
using Item.Display.ClickHandlers;
using Item.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;
using Items.Inventory;
using UI.ToolTip;
using Unity.VisualScripting;


namespace PlayerModule.KeyPress {

    public class PlayerKeyPress : MonoBehaviour
    {
        private PlayerInventory playerInventory;
        // Start is called before the first frame update
        void Start()
        {
            playerInventory = GetComponent<PlayerInventory>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.E)) {
                ToolTipController.Instance.HideToolTip();
                playerInventory.toggleInventory();
            }
            /*
            if (Input.GetKeyDown(KeyCode.C)) {
                playerInventory.toggleToolAndInventory();
            }
            */

            inventoryNavigationKeys();
            inventoryKeyPresses();

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