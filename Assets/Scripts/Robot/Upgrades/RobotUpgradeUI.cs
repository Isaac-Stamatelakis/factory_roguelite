using System;
using System.IO;
using DevTools.Upgrades;
using Newtonsoft.Json;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using WorldModule;

namespace Robot.Upgrades
{
    public class RobotUpgradeUI : MonoBehaviour
    {
        [SerializeField] private RobotUpgradeNetworkUI robotUpgradeNetworkUI;
        [SerializeField] private RobotUpgradeNodeContentUI robotUpgradeNodeContentUI;
        private RobotUpgradeNodeNetwork nodeNetwork;
        private DevToolUpgradeInfo upgradeInfo;
        private Vector3 contentLocation;

        public void Start()
        {
            contentLocation = robotUpgradeNodeContentUI.transform.position;
            var vector3 = robotUpgradeNodeContentUI.transform.localPosition;
            vector3.x = 0;
            robotUpgradeNodeContentUI.transform.localPosition = vector3;
        }

        public void Initialize(RobotUpgradeNodeNetwork nodeNetwork)
        {
            this.nodeNetwork = nodeNetwork;
            robotUpgradeNodeContentUI.Initialize(this.nodeNetwork);
            robotUpgradeNetworkUI.Initialize(this,nodeNetwork);
        }

        internal void SetUpgradeInfo(DevToolUpgradeInfo upgradeInfo)
        {
            this.upgradeInfo = upgradeInfo;
        }

        public void DisplayNodeContent(RobotUpgradeNode robotUpgradeNode)
        {
            if (robotUpgradeNodeContentUI.UnActivated)
            {
                StartCoroutine(UIUtils.TransitionUIElement((RectTransform)this.robotUpgradeNodeContentUI.transform, contentLocation));
            }
            robotUpgradeNodeContentUI.DisplayUpgradeNode(robotUpgradeNode);
            
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
