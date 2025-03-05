using System.Collections.Generic;
using System.IO;
using DevTools.Structures;
using UnityEngine;

namespace DevTools.Upgrades
{
    public class DevToolUpgradeSelector : DevToolListUI
    {
        [SerializeField] private DevToolUpgradeListElement upgradeListElementPrefab;
        [SerializeField] private DevToolNewUpgradePopUp newUpgradePopUpPrefab;
        protected override void OnAddButtonClick()
        {
            DevToolNewUpgradePopUp newUpgradePopUp = Instantiate(newUpgradePopUpPrefab, transform, false);
            newUpgradePopUp.Initialize(this);
        }

        public override void DisplayList()
        {
            GlobalHelper.deleteAllChildren(mList.transform);
            string path = DevToolUtils.GetDevToolPath(DevTool.Upgrade);
            string[] files = Directory.GetFiles(path);
            
            foreach (string file in files)
            {
                if (file.Contains(".meta")) continue;
                DevToolUpgradeInfo info = new DevToolUpgradeInfo(file);
                DevToolUpgradeListElement devToolUpgradeListElement = Instantiate(upgradeListElementPrefab,mList.transform);
                devToolUpgradeListElement.Display(this,info);
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
