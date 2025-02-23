using System.IO;
using Newtonsoft.Json;
using Robot.Upgrades;
using TMPro;
using UI;
using UI.NodeNetwork;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace DevTools.Upgrades
{
    public class DevToolUpgradeListElement : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private RobotUpgradeUI robotUpgradeNetworkUIPrefab;
        [SerializeField] private TextMeshProUGUI mNameText;
        private DevToolUpgradeInfo upgradeInfo;
        private DevToolUpgradeSelector parent;
        internal void Display(DevToolUpgradeSelector parent, DevToolUpgradeInfo upgradeInfo)
        {
            this.upgradeInfo = upgradeInfo;
            mNameText.text = Path.GetFileName(upgradeInfo.Path).Replace(".bin","");
            this.parent = parent;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SerializedRobotUpgradeNodeNetwork sNetwork = RobotUpgradeUtils.DeserializeRobotNodeNetwork(File.ReadAllBytes(upgradeInfo.Path));
            RobotUpgradeUI robotUpgradeUI = Instantiate(robotUpgradeNetworkUIPrefab);
            RobotUpgradeNodeNetwork robotUpgradeNodeNetwork = RobotUpgradeUtils.FromSerializedNetwork(sNetwork);
            if (robotUpgradeNodeNetwork == null)
            {
                Debug.LogError($"Upgrade data at path '{upgradeInfo.Path}' is corrupted :(");
                return;
            }
            robotUpgradeUI.Initialize(mNameText.text,robotUpgradeNodeNetwork);
            robotUpgradeUI.SetUpgradeInfo(upgradeInfo);
            CanvasController.Instance.DisplayObject(robotUpgradeUI.gameObject);
            
        }
    }
}
