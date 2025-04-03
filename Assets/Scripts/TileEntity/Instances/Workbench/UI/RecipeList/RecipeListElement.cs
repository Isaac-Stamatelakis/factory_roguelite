using System;
using System.Collections.Generic;
using Item.Inventory;
using Item.Slot;
using Items;
using Items.Inventory;
using Recipe.Objects;
using Recipe.Viewer;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TileEntity.Instances.Workbench.UI.RecipeList
{
    public class RecipeListElement : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private InventoryUI mInventoryUI;
        private RecipeLookUpList recipeLookUpListParent;
        private Action<int> clickAction;
        private int index;
        private string itemName;
        
        public void Display(List<ItemSlot> outputs, RecipeObject recipeObject, Action<int> clickAction, int index, string itemName)
        {
            this.index = index;
            this.clickAction = clickAction;
            this.itemName = itemName;
            
            mInventoryUI.InventoryInteractMode = InventoryInteractMode.UnInteractable;
            mInventoryUI.OverrideClickAction(clickAction);
            
            if (outputs.Count == 1)
            {
                mInventoryUI.DisplayInventory(new List<ItemSlot>{outputs[0]},false);
                return;
            }
            InventoryUIRotator rotator = mInventoryUI.GetComponent<InventoryUIRotator>();
            if (ReferenceEquals(rotator, null))
            {
                rotator = mInventoryUI.gameObject.AddComponent<InventoryUIRotator>();
            }

            List<List<ItemSlot>> inventoriesList = new List<List<ItemSlot>>();
            foreach (ItemSlot itemSlot in outputs)
            {
                inventoriesList.Add(new List<ItemSlot>{itemSlot});
            }
            
            rotator.Initialize(inventoriesList,1,100,false);
            
        }

        public void SetColor(Color color)
        {
            GetComponent<Image>().color = color;
        }

        public bool Filter(string text)
        {
            return itemName.ToLower().Contains(text.ToLower());
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            clickAction.Invoke(index);
        }
    }
}
