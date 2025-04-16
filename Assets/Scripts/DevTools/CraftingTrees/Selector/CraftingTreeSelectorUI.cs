using System.IO;
using DevTools.CraftingTrees.Network;
using DevTools.Structures;
using DevTools.Upgrades;
using Newtonsoft.Json;
using UI;
using UnityEngine;

namespace DevTools.CraftingTrees.Selector
{
    public class CraftingTreeSelectorUI : DevToolListUI
    {
        [SerializeField] private CraftingTreeSelectorUIElement elementPrefab;
        [SerializeField] private NewCraftingTreePopUpUI newCraftingTreePopUpPrefab;
        [SerializeField] private CraftingTreeGeneratorUI craftingTreeGeneratorPrefab;
        
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

                SerializedCraftingTreeNodeNetwork serializedCraftingTreeNodeNetwork = GlobalHelper.DeserializeCompressedJson<SerializedCraftingTreeNodeNetwork>(file);
                CraftingTreeNodeNetwork craftingTreeNodeNetwork = SerializedCraftingTreeNodeNetworkUtils.DeserializeNodeNetwork(serializedCraftingTreeNodeNetwork);
                if (craftingTreeNodeNetwork == null)
                {
                    Debug.LogError($"Crafting tree data at path '{file}' is corrupted :(");
                    return;
                }
                CraftingTreeGeneratorUI craftingTreeGeneratorUI = Instantiate(craftingTreeGeneratorPrefab,mList.transform);
                craftingTreeGeneratorUI.Initialize(craftingTreeNodeNetwork,file);
                CanvasController.Instance.DisplayObject(craftingTreeGeneratorUI.gameObject);
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
