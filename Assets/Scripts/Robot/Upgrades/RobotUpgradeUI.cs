using System;
using System.IO;
using DevTools.Upgrades;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using WorldModule;

namespace Robot.Upgrades
{
    public class RobotUpgradeUI : MonoBehaviour
    {
        [SerializeField] private RobotUpgradeNetworkUI robotUpgradeNetworkUI;
        private RobotUpgradeNodeNetwork nodeNetwork;
        private DevToolUpgradeInfo upgradeInfo;
        public void Initialize(RobotUpgradeNodeNetwork nodeNetwork)
        {
            this.nodeNetwork = nodeNetwork;
            robotUpgradeNetworkUI.Initialize(this,nodeNetwork);
        }

        internal void SetUpgradeInfo(DevToolUpgradeInfo upgradeInfo)
        {
            this.upgradeInfo = upgradeInfo;
        }

        public void DisplayNodeContent(RobotUpgradeNode robotUpgradeNode)
        {
            
        }

        public void OnDestroy()
        {
            string path = upgradeInfo.Path;
            if (path == null) return;
            if (SceneManager.GetActiveScene().name != "DevTools")
            {
                Debug.LogWarning("Tried to edit robot upgrade network outside of dev tools");
                return;
            }
            string json = JsonConvert.SerializeObject(nodeNetwork);
            byte[] bytes = WorldLoadUtils.CompressString(json);
            File.WriteAllBytes(path, bytes);
        }
    }
}
