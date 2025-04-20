using System;
using System.IO;
using TileEntity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DevTools.CraftingTrees.Selector
{
    public class CraftingTreeSelectorUIElement : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI mTextElement;
        [SerializeField] private Button mEditButton;
        private Action<int> onClick;
        private int index;

        public void Display(CraftingTreeSelectorUI.CraftingTreeInfo craftingTreeInfo, Action<int> onClick, int index)
        {
            this.index = index;
            switch (craftingTreeInfo.DisplayMode)
            {
                case CraftingTreeSelectorUI.DisplayMode.Folder:
                    Tier tier = (Tier) craftingTreeInfo.Tier;
                    mTextElement.text = tier.ToString();
                    break;
                case CraftingTreeSelectorUI.DisplayMode.File:
                    mTextElement.text = Path.GetFileName(craftingTreeInfo.Path).Replace(".bin","");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            this.onClick = onClick;
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke(index);
        }
    }
}
