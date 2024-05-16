using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using RecipeModule.Viewer;
using Items;

namespace UI.JEI {
    public class CatalogueElementClickHandler : MonoBehaviour, IPointerClickHandler
    {
        private ItemCatalogueController catalogueController;
        private ItemObject itemObject;
        public void init(ItemCatalogueController catalogueController, ItemObject itemObject) {
            this.catalogueController = catalogueController;
            this.itemObject = itemObject;
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) {
                switch (catalogueController.Mode) {
                    case CatalogueMode.Recipe:
                        RecipeViewerHelper.displayCraftingOfItem(itemObject);
                        break;
                    case CatalogueMode.Cheat:
                        ItemSlot itemSlot = ItemSlotFactory.createNewItemSlot(itemObject,Global.MaxSize);
                        GrabbedItemProperties grabbedItemProperties = GrabbedItemProperties.Instance;
                        grabbedItemProperties.setItemSlot(itemSlot);
                        break;
                }
            } else if (eventData.button == PointerEventData.InputButton.Right) {
                switch (catalogueController.Mode) {
                    case CatalogueMode.Recipe:
                        RecipeViewerHelper.displayUsesOfItem(itemObject);
                        break;
                    case CatalogueMode.Cheat:
                        ItemSlot itemSlot = ItemSlotFactory.createNewItemSlot(itemObject,1);
                        GrabbedItemProperties grabbedItemProperties = GameObject.Find("GrabbedItem").GetComponent<GrabbedItemProperties>();
                        if (grabbedItemProperties.ItemSlot == null) {
                            grabbedItemProperties.setItemSlot(itemSlot);
                        } else {
                            if (grabbedItemProperties.ItemSlot.itemObject.id == itemObject.id) {
                                grabbedItemProperties.ItemSlot.amount = Mathf.Min(Global.MaxSize,1+grabbedItemProperties.ItemSlot.amount);
                            } else {
                                grabbedItemProperties.setItemSlot(itemSlot);
                            }
                        }
                        grabbedItemProperties.updateSprite();
                        break;
                }
            }
        }

    }
}

