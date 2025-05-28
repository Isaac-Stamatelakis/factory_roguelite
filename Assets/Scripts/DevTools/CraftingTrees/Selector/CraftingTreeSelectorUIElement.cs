using System;
using System.IO;
using TileEntity;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DevTools.CraftingTrees.Selector
{
    public class CraftingTreeSelectorUIElement : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI mTextElement;
        [SerializeField] private Button mEditButton;
        [SerializeField] private CraftingTreeFileEditorUI mFileEditorPrefab;
        private Action<int> onClick;
        private int index;

        public void Display(CraftingTreeSelectorUI.CraftingTreeInfo craftingTreeInfo, Action<int> onClick, int index, Action refresh)
        {
            mEditButton.gameObject.SetActive(craftingTreeInfo.DisplayMode==CraftingTreeSelectorUI.DisplayMode.File);
            this.index = index;
            switch (craftingTreeInfo.DisplayMode)
            {
                case CraftingTreeSelectorUI.DisplayMode.Folder:
                    Tier tier = (Tier) craftingTreeInfo.Tier;
                    mTextElement.text = tier.ToString();
                    break;
                case CraftingTreeSelectorUI.DisplayMode.File:
                    mTextElement.text = Path.GetFileName(craftingTreeInfo.Path).Replace(".json","");
                    mEditButton.onClick.AddListener(() =>
                    {
                        CraftingTreeFileEditorUI craftingTreeEditor = Instantiate(mFileEditorPrefab);
                        craftingTreeEditor.Display(craftingTreeInfo.Path,refresh);
                        CanvasController.Instance.DisplayObject(craftingTreeEditor.gameObject,hideParent:false);
                    });
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
