using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Items.Inventory;


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
            if (EventSystem.current.currentSelectedGameObject != null) {
                return;
            }
            if (Input.GetKeyDown(KeyCode.E)) {
                playerInventory.changeDisplayMode(InventoryDisplayMode.Inventory);
                playerInventory.toggleInventory();
            }
            if (Input.GetKeyDown(KeyCode.C)) {
                playerInventory.toggleToolAndInventory();
            }

            inventoryNavigationKeys();
            inventoryKeyPresses();

        }

        private void inventoryNavigationKeys() {
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                playerInventory.changeSelectedSlot(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2)) {
                playerInventory.changeSelectedSlot(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3)) {
                playerInventory.changeSelectedSlot(2);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4)) {
                playerInventory.changeSelectedSlot(3);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5)) {
                playerInventory.changeSelectedSlot(4);
            }
            if (Input.GetKeyDown(KeyCode.Alpha6)) {
                playerInventory.changeSelectedSlot(5);
            }
            if (Input.GetKeyDown(KeyCode.Alpha7)) {
                playerInventory.changeSelectedSlot(6);
            }
            if (Input.GetKeyDown(KeyCode.Alpha8)) {
                playerInventory.changeSelectedSlot(7);
            }
            if (Input.GetKeyDown(KeyCode.Alpha9)) {
                playerInventory.changeSelectedSlot(8);
            }
            if (Input.GetKeyDown(KeyCode.Alpha0)) {
                playerInventory.changeSelectedSlot(9);
            }
        }

        private void inventoryKeyPresses() {
            if (Input.GetKey(KeyCode.R)) {
                GameObject current = EventSystem.current.currentSelectedGameObject;
                if (current != null) {
                    ItemSlotUIClickHandler clickHandler = current.GetComponent<ItemSlotUIClickHandler>();
                    if (clickHandler != null) {
                        ((IItemSlotUIElement) clickHandler).showRecipes();
                    } 
                }
            }
            if (Input.GetKey(KeyCode.U)) {
                GameObject current = EventSystem.current.currentSelectedGameObject;
                if (current != null) {
                    ItemSlotUIClickHandler clickHandler = current.GetComponent<ItemSlotUIClickHandler>();
                    if (clickHandler != null) {
                        ((IItemSlotUIElement) clickHandler).showUses();
                    } 
                }
            }
        }

        private void controls() {

        }
    }
}