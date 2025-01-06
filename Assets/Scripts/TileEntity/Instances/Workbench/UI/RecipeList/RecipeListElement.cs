using System.Collections.Generic;
using Item.Slot;
using Items;
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
        [SerializeField] private ItemSlotUI mItemSlotUI;
        [SerializeField] private TextMeshProUGUI mNameText;
        private RecipeLookUpList recipeLookUpListParent;
        private int mode;
        private int index;
        private RecipeListHeader header;
        public bool HeaderActive => header.ElementsVisible;

        public void Display(ItemSlot output, RecipeObject recipeObject, RecipeLookUpList recipeLookUpList, RecipeListHeader header, int mode, int index)
        {
            recipeLookUpListParent = recipeLookUpList;
            this.mode = mode;
            this.index = index;
            mItemSlotUI.Display(output);
            mNameText.text = output.itemObject.name;
            this.header = header;
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
