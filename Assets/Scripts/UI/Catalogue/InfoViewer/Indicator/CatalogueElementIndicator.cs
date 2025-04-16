using System;
using System.Collections.Generic;
using Item.Slot;
using Items.Inventory;
using Robot.Tool;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Catalogue.InfoViewer.Indicator
{
    public class CatalogueElementIndicator : MonoBehaviour
    {
        [SerializeField] private InventoryUI mInventoryUI;
        private int nodeCount;
        private CatalogueInfoViewer infoViewerParent;
        public void Initialize(CatalogueInfoViewer catalogueInfoViewer)
        {
            infoViewerParent = catalogueInfoViewer;
            nodeCount = mInventoryUI.transform.childCount;
            mInventoryUI.InventoryInteractMode = InventoryInteractMode.OverrideAction;
            mInventoryUI.OverrideClickAction(ClickOverride);
            mInventoryUI.SetToolTipOverride(ToolTipOverride);
            
            return;
            void ClickOverride(PointerEventData.InputButton inputButton, int index)
            {
                int displayOffset = index - nodeCount / 2;
                ICatalogueElement catalogueElement = infoViewerParent.GetAdjacentDisplayElement(displayOffset);
                switch (inputButton)
                {
                    case PointerEventData.InputButton.Left:
<<<<<<< HEAD
<<<<<<< HEAD
                        infoViewerParent.MoveDisplayElement(displayOffset);
=======
                        infoViewerParent.MoveDisplayElement(index);
>>>>>>> 286e87ab (Added crafting tree dev tool prep, catalgoue control navigator now displays itemslots instead of images)
=======
                        infoViewerParent.MoveDisplayElement(displayOffset);
>>>>>>> 56642417 (Added crafting tree data structures, item slot uis now constantly update tooltip when focused)
                        break;
                    case PointerEventData.InputButton.Right:
                        catalogueElement.DisplayAllElements(infoViewerParent.GameStageCollection);
                        break;
                    case PointerEventData.InputButton.Middle:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(inputButton), inputButton, null);
                }
            }

            string ToolTipOverride(int index)
            {
                int displayOffset = index - nodeCount / 2;
                ICatalogueElement catalogueElement = infoViewerParent.GetAdjacentDisplayElement(displayOffset);
                return catalogueElement.GetName();
            }
        }

        

        public void DisplayNodes()
        {
            List<ItemSlot> displayedItems = new List<ItemSlot>();
            for (var index = 0; index < nodeCount; index++)
            {
                int displayOffset = index - nodeCount / 2;
                ICatalogueElement catalogueElement = infoViewerParent.GetAdjacentDisplayElement(displayOffset);
                ItemSlot itemSlot = new ItemSlot(catalogueElement.GetDisplayItem(), 1, null);
                displayedItems.Add(itemSlot);
            }
            mInventoryUI.DisplayInventory(displayedItems,clear:false);
        }
    }
}
