using System.IO;
<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> 1420320e (Added basic ui for crafting trees)
using DevTools.CraftingTrees.Network;
using DevTools.Structures;
using DevTools.Upgrades;
using Newtonsoft.Json;
using UI;
<<<<<<< HEAD
=======
using DevTools.Structures;
>>>>>>> 286e87ab (Added crafting tree dev tool prep, catalgoue control navigator now displays itemslots instead of images)
=======
>>>>>>> 1420320e (Added basic ui for crafting trees)
using UnityEngine;

namespace DevTools.CraftingTrees.Selector
{
    public class CraftingTreeSelectorUI : DevToolListUI
    {
        [SerializeField] private CraftingTreeSelectorUIElement elementPrefab;
        [SerializeField] private NewCraftingTreePopUpUI newCraftingTreePopUpPrefab;
<<<<<<< HEAD
<<<<<<< HEAD
        [SerializeField] private CraftingTreeGeneratorUI craftingTreeGeneratorPrefab;
=======

>>>>>>> 286e87ab (Added crafting tree dev tool prep, catalgoue control navigator now displays itemslots instead of images)
=======
        [SerializeField] private CraftingTreeGeneratorUI craftingTreeGeneratorPrefab;
>>>>>>> 1420320e (Added basic ui for crafting trees)
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
<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> 1420320e (Added basic ui for crafting trees)
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
<<<<<<< HEAD
=======
                Debug.Log(file);
>>>>>>> 286e87ab (Added crafting tree dev tool prep, catalgoue control navigator now displays itemslots instead of images)
=======
>>>>>>> 1420320e (Added basic ui for crafting trees)
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
