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
        [SerializeField] private TextMeshProUGUI mNameText;
        private RecipeLookUpList recipeLookUpListParent;
        private int mode;
        private int index;
        private RecipeListHeader header;
        public bool HeaderActive => header.ElementsVisible;
        
        public void Display(List<ItemSlot> outputs, RecipeObject recipeObject, RecipeLookUpList recipeLookUpList, RecipeListHeader header, int mode, int index)
        {
            recipeLookUpListParent = recipeLookUpList;
            this.mode = mode;
            this.index = index;
            this.header = header;
            
            if (outputs.Count == 1)
            {
                mInventoryUI.DisplayInventory(new List<ItemSlot>{outputs[0]},false);
                mNameText.text = outputs[0].itemObject.name;
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
            mNameText.text = recipeObject.name;
            
        }

        public void SetColor(Color color)
        {
            GetComponent<Image>().color = color;
        }

        public bool Filter(string text)
        {
            return mNameText.text.ToLower().Contains(text.ToLower());
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            recipeLookUpListParent.Select(mode,index);
        }
    }
}
