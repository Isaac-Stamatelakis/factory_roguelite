using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DevTools.CraftingTrees.Selector
{
    public class CraftingTreeSelectorUIElement : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI mTextElement;
        private Action<int> onClick;
        private int index;

        public void Display(CraftingTreeSelectorUI.CraftingTreeInfo craftingTreeInfo, Action<int> onClick, int index)
        {
            this.index = index;
            mTextElement.text = Path.GetFileName(craftingTreeInfo.Path).Replace(".bin","");
            this.onClick = onClick;
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke(index);
        }
    }
}
