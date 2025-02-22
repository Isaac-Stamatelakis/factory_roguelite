using System.Collections.Generic;
using System.IO;
using DevTools.Structures;
using UnityEngine;

namespace DevTools.Upgrades
{
    public class DevToolUpgradeSelector : DevToolListUI
    {
        [SerializeField] private DevToolUpgradeListElement upgradeListElementPrefab;
        protected override void OnAddButtonClick()
        {
            
        }

        public override void DisplayList()
        {
            string path = DevToolUtils.GetDevToolPath(DevTool.Upgrade);
            string[] folders = Directory.GetDirectories(path);
            
            foreach (string folder in folders)
            {
                DevToolUpgradeInfo info = new DevToolUpgradeInfo(folder);
                DevToolUpgradeListElement devToolUpgradeListElement = Instantiate(upgradeListElementPrefab,mList.transform);
                devToolUpgradeListElement.Display(info);
                
            }
        }
    }
    internal struct DevToolUpgradeInfo
    {
        public string Path;

        public DevToolUpgradeInfo(string path)
        {
            Path = path;
        }
    }
}
