using System;
using System.Collections.Generic;
using System.IO;
using DevTools.CraftingTrees.Network;
using DevTools.Structures;
using DevTools.Upgrades;
using Newtonsoft.Json;
using TileEntity;
using UI;
using UnityEditor;
using UnityEngine;

namespace DevTools.CraftingTrees.Selector
{
    public interface IDevToolBackButtonOverrideUI
    {
        public bool BackPressOverriden();
    }
    public class CraftingTreeSelectorUI : DevToolListUI, IDevToolBackButtonOverrideUI
    {
        public enum DisplayMode
        {
            Folder,
            File
        }

        private const string TIER_PREFIX = "Tier";
        private const int INVALID_TIER = -1;

        private string currentFolder;
        public string CurrentFolder => currentFolder;
        private DisplayMode displayMode;
        [SerializeField] private CraftingTreeSelectorUIElement elementPrefab;
        [SerializeField] private NewCraftingTreePopUpUI newCraftingTreePopUpPrefab;
        [SerializeField] private CraftingTreeGeneratorUI craftingTreeGeneratorPrefab;

        protected override void OnAddButtonClick()
        {
            if (displayMode == DisplayMode.Folder) return;
            NewCraftingTreePopUpUI newUpgradePopUp = Instantiate(newCraftingTreePopUpPrefab, transform, false);
            newUpgradePopUp.Initialize(this);
        }

        public override void DisplayList()
        {
            mAddButton.gameObject.SetActive(displayMode == DisplayMode.File);
            GlobalHelper.DeleteAllChildren(mList.transform);
            string[] paths;
            switch(displayMode)
            {
                case DisplayMode.Folder:
                    string path = DevToolUtils.GetDevToolPath(DevTool.CraftingTree);
                    paths = Directory.GetDirectories(path);
                    List<Tier> containedTiers = new List<Tier>();
                    foreach (string possibleFolder in paths)
                    {
                        int tierValue = GetTierFromFolder(possibleFolder);
                        if (tierValue == INVALID_TIER) continue;
                        containedTiers.Add((Tier)tierValue);
                    }
                    Tier[] tiers = System.Enum.GetValues(typeof(Tier)) as Tier[];
                    for (int i = 0; i < tiers.Length; i++)
                    {
                        if (containedTiers.Contains(tiers[i])) continue;
                        Tier tier = tiers[i];
                        string tierName = TIER_PREFIX + (int)tier;
                        Directory.CreateDirectory(Path.Combine(path, tierName));
                    }
                    break;
                case DisplayMode.File:
                    paths = Directory.GetFiles(currentFolder);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            

            void OnSelect(int index)
            {
                
                string elementPath = paths[index];
                switch (displayMode)
                {
                    case DisplayMode.Folder:
                        currentFolder = elementPath;
                        displayMode = DisplayMode.File;
                        DisplayList();
                        break;
                    case DisplayMode.File:
                        SerializedCraftingTreeNodeNetwork serializedCraftingTreeNodeNetwork = GlobalHelper.DeserializeCompressedJson<SerializedCraftingTreeNodeNetwork>(elementPath);
                        CraftingTreeNodeNetwork craftingTreeNodeNetwork = SerializedCraftingTreeNodeNetworkUtils.DeserializeNodeNetwork(serializedCraftingTreeNodeNetwork);
                        if (craftingTreeNodeNetwork == null)
                        {
                            Debug.LogError($"Crafting tree data at path '{elementPath}' is corrupted :(");
                            return;
                        }
                        CraftingTreeGeneratorUI craftingTreeGeneratorUI = Instantiate(craftingTreeGeneratorPrefab,mList.transform);
                        craftingTreeGeneratorUI.Initialize(craftingTreeNodeNetwork,elementPath);
                        CanvasController.Instance.DisplayObject(craftingTreeGeneratorUI.gameObject);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                

            }

            for (var index = 0; index < paths.Length; index++)
            {
                var elementPath = paths[index];
                if (elementPath.Contains(".meta")) continue;
                CraftingTreeInfo info = new CraftingTreeInfo
                {
                    Path = elementPath,
                    DisplayMode = displayMode
                };
                if (displayMode == DisplayMode.Folder)
                {
                    int tier = GetTierFromFolder(elementPath);
                    if (tier == INVALID_TIER) continue;
                    info.Tier = tier;
                }
                
                CraftingTreeSelectorUIElement devToolUpgradeListElement = Instantiate(elementPrefab, mList.transform);
                devToolUpgradeListElement.Display(info, OnSelect,index);
            }
        }

        private int GetTierFromFolder(string elementPath)
        {
            string directoryName = Path.GetFileName(elementPath);
            if (string.IsNullOrEmpty(directoryName)) return INVALID_TIER;
            string tierIntString = directoryName.Replace("Tier", "");
            if (!int.TryParse(tierIntString, out int result)) return INVALID_TIER;
            return result;
        }

        public struct CraftingTreeInfo
        {
            public string Path;
            public int Tier;
            public DisplayMode DisplayMode;
        }

        public bool BackPressOverriden()
        {
            if (displayMode == DisplayMode.Folder)
            {
                return false;
            }
            displayMode = DisplayMode.Folder;
            DisplayList();
            return true;
            
        }
    }
}
