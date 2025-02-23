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
            RobotUpgradeNodeNetwork nodeNetwork = RobotUpgradeUtils.DeserializeRobotNodeNetwork(File.ReadAllBytes(upgradeInfo.Path));
            RobotUpgradeUI robotUpgradeUI = Instantiate(robotUpgradeNetworkUIPrefab);
            robotUpgradeUI.Initialize(nodeNetwork);
            robotUpgradeUI.SetUpgradeInfo(upgradeInfo);
            CanvasController.Instance.DisplayObject(robotUpgradeUI.gameObject);
            
        }
    }
}
