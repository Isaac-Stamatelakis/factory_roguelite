using System.IO;
using DevTools.Structures;
using UnityEngine;

namespace DevTools.CraftingTrees.Selector
{
    public class CraftingTreeSelectorUI : DevToolListUI
    {
        [SerializeField] private CraftingTreeSelectorUIElement elementPrefab;
        [SerializeField] private NewCraftingTreePopUpUI newCraftingTreePopUpPrefab;

        protected override void OnAddButtonClick()
        {
            NewCraftingTreePopUpUI newUpgradePopUp = Instantiate(newCraftingTreePopUpPrefab, transform, false);
            newUpgradePopUp.Initialize(this);
        }

        public override void DisplayList()
        {
            GlobalHelper.DeleteAllChildren(mList.transform);
            string path = DevToolUtils.GetDevToolPath(DevTool.CraftingTree);
            string[] files = Directory.GetFiles(path);

            void OnSelect(int index)
            {
                string file = files[index];
                Debug.Log(file);
            }

            for (var index = 0; index < files.Length; index++)
            {
                var file = files[index];
                if (file.Contains(".meta")) continue;
                CraftingTreeInfo info = new CraftingTreeInfo
                {
                    Path = file,
                };
                CraftingTreeSelectorUIElement devToolUpgradeListElement = Instantiate(elementPrefab, mList.transform);
                devToolUpgradeListElement.Display(info, OnSelect,index);
            }
        }

        public struct CraftingTreeInfo
        {
            public string Path;
        }
    }
}
