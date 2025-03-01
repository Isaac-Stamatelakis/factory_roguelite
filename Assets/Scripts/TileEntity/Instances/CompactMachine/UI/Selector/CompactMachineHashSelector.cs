using System;
using System.Collections.Generic;
using TileEntity.Instances.CompactMachines;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TileEntity.Instances.CompactMachine.UI.Selector
{
    public class CompactMachineHashSelector : MonoBehaviour
    {
        [SerializeField] private CompactMachineHashSelectorElement mElementPrefab;
        [SerializeField] private VerticalLayoutGroup mList;
        [SerializeField] private TMP_InputField mSearchField;

        List<CompactMachineHashSelectorElement> displayElements = new List<CompactMachineHashSelectorElement>();
        private string lastSearch = string.Empty;
        public void Start()
        {
            mSearchField.onValueChanged.AddListener((value) =>
            {
                string search = value.ToLower();
                bool searchIncrease = search.Length > lastSearch.Length;
                foreach (CompactMachineHashSelectorElement element in displayElements)
                {
                    if (searchIncrease && !element.gameObject.activeInHierarchy) continue;
                    HashSelectorDisplayData displayData = element.DisplayData;
                    bool match = displayData.MetaData.Name.ToLower().Contains(search) || displayData.Hash.ToLower().Contains(search);
                    element.gameObject.SetActive(match);
                }

                lastSearch = search;

            });
        }

        public void Display(Action<string> onHashSelect)
        {
            displayElements.Clear();
            GlobalHelper.deleteAllChildren(mList.transform);
            string[] hashs = CompactMachineUtils.GetAllHashes();
            foreach (string hash in hashs)
            {
                CompactMachineMetaData metaData = CompactMachineUtils.GetMetaDataFromHash(hash);
                if (metaData == null || !metaData.Locked) continue;
                HashSelectorDisplayData displayData = new HashSelectorDisplayData(hash, metaData, onHashSelect);
                CompactMachineHashSelectorElement newElement = Instantiate(mElementPrefab, mList.transform);
                newElement.Display(displayData);
                displayElements.Add(newElement);
            }
        }
    }

    internal struct HashSelectorDisplayData
    {
        public string Hash;
        public CompactMachineMetaData MetaData;
        public Action<string> OnHashSelect;

        public HashSelectorDisplayData(string hash, CompactMachineMetaData metaData, Action<string> onHashSelect)
        {
            Hash = hash;
            MetaData = metaData;
            OnHashSelect = onHashSelect;
        }
    }
}
