using System.Collections;
using System.Collections.Generic;
using Item.Inventory;
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

        private void inventoryKeyPresses() {
            if (Input.GetKey(KeyCode.R)) {
                GameObject current = EventSystem.current.currentSelectedGameObject;
                if (current != null) {
                    ItemSlotUIClickHandler clickHandler = current.GetComponent<ItemSlotUIClickHandler>();
                    if (clickHandler != null) {
                        clickHandler.ShowRecipes();
                    } 
                }
            }
            if (Input.GetKey(KeyCode.U)) {
                GameObject current = EventSystem.current.currentSelectedGameObject;
                if (current != null) {
                    ItemSlotUIClickHandler clickHandler = current.GetComponent<ItemSlotUIClickHandler>();
                    if (clickHandler != null) {
                        clickHandler.ShowUses();
                    } 
                }
            }
        }

        private void controls() {

        }
    }
}