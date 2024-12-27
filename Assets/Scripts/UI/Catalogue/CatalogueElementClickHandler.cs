using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using RecipeModule.Viewer;
using Items;
using Items.Tags;
using Recipe.Viewer;

namespace UI.JEI {
    public class CatalogueElementClickHandler : MonoBehaviour, IPointerClickHandler
    {
        private ItemCatalogueController catalogueController;
        private ItemSlot itemSlot;
        public void init(ItemCatalogueController catalogueController, ItemSlot itemSlot) {
            this.catalogueController = catalogueController;
            this.itemSlot = itemSlot;
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) {
                switch (catalogueController.Mode) {
                    case CatalogueMode.Recipe:
                        RecipeViewerHelper.DisplayCraftingOfItem(itemSlot);
                        break;
                    case CatalogueMode.Cheat:
                        ItemSlot copy = ItemSlotFactory.Splice(itemSlot,Global.MaxSize);
                        GrabbedItemProperties grabbedItemProperties = GrabbedItemProperties.Instance;
                        grabbedItemProperties.SetItemSlot(copy);
                        break;
                }
            } else if (eventData.button == PointerEventData.InputButton.Right) {
                switch (catalogueController.Mode) {
                    case CatalogueMode.Recipe:
                        RecipeViewerHelper.DisplayUsesOfItem(itemSlot);
                        break;
                    case CatalogueMode.Cheat:
                        ItemSlot copy = ItemSlotFactory.Splice(itemSlot,1);
                        GrabbedItemProperties grabbedItemProperties = GrabbedItemProperties.Instance;
                        if (grabbedItemProperties.ItemSlot == null || grabbedItemProperties.ItemSlot.itemObject == null) {
                            grabbedItemProperties.SetItemSlot(copy);
                        } else {
                            if (grabbedItemProperties.ItemSlot.itemObject.id == itemSlot.itemObject.id) {
                                grabbedItemProperties.ItemSlot.amount = GlobalHelper.MinUInt(Global.MaxSize, 1+grabbedItemProperties.ItemSlot.amount);
                            } else {
                                grabbedItemProperties.SetItemSlot(itemSlot);
                            }
                        }
                        break;
                }
            }
        }

    }
}

