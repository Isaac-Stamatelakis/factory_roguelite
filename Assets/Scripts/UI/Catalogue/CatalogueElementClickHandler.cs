using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using RecipeModule.Viewer;

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
                        GrabbedItemProperties grabbedItemProperties = GameObject.Find("GrabbedItem").GetComponent<GrabbedItemProperties>();
                        grabbedItemProperties.itemSlot = itemSlot;
                        grabbedItemProperties.updateSprite();
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
                        if (grabbedItemProperties.itemSlot == null) {
                            grabbedItemProperties.itemSlot = itemSlot;
                        } else {
                            if (grabbedItemProperties.itemSlot.itemObject.id == itemObject.id) {
                                grabbedItemProperties.itemSlot.amount = Mathf.Min(Global.MaxSize,1+grabbedItemProperties.itemSlot.amount);
                            } else {
                                grabbedItemProperties.itemSlot = itemSlot;
                            }
                        }
                        grabbedItemProperties.updateSprite();
                        break;
                }
            }
        }

    }
}

