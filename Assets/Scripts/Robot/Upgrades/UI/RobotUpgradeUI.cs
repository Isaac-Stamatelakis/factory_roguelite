using System;
using System.IO;
using DevTools.Upgrades;
using Newtonsoft.Json;
using Robot.Upgrades.Network;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using WorldModule;

namespace Robot.Upgrades
{
    public class RobotUpgradeUI : MonoBehaviour, IPointerClickHandler
    {
        [FormerlySerializedAs("robotUpgradeNetworkUI")] [SerializeField] private RobotUpgradeNetworkUI mRobotUpgradeNetworkUI;
        [FormerlySerializedAs("robotUpgradeNodeContentUI")] [SerializeField] private RobotUpgradeNodeContentUI mRobotUpgradeNodeContentUI;
        [SerializeField] private TextMeshProUGUI mTitleText;
        private RobotUpgradeNodeNetwork nodeNetwork;
        private DevToolUpgradeInfo upgradeInfo;
        private Vector3 contentLocation;

        public void Start()
        {
            contentLocation = mRobotUpgradeNodeContentUI.transform.position;
            var vector3 = mRobotUpgradeNodeContentUI.transform.localPosition;
            vector3.x = 0;
            mRobotUpgradeNodeContentUI.transform.localPosition = vector3;
            mRobotUpgradeNodeContentUI.DisableEditElements();
        }

        public void Initialize(string networkName, RobotUpgradeNodeNetwork nodeNetwork)
        {
            this.nodeNetwork = nodeNetwork;
            mRobotUpgradeNodeContentUI.Initialize(this.mRobotUpgradeNetworkUI, this.nodeNetwork);
            mRobotUpgradeNetworkUI.Initialize(this,nodeNetwork);
            

            mTitleText.text = networkName;
        }

        internal void SetUpgradeInfo(DevToolUpgradeInfo upgradeInfo)
        {
            this.upgradeInfo = upgradeInfo;
        }

        public void DisplayNodeContent(RobotUpgradeNode robotUpgradeNode)
        {
            if (robotUpgradeNode == null)
            {
                if (!mRobotUpgradeNodeContentUI.UnActivated)
                {
                    Vector3 destination = new Vector3(0,mRobotUpgradeNodeContentUI.transform.localPosition.y, 0);
                    StartCoroutine(UIUtils.TransitionUIElement((RectTransform)this.mRobotUpgradeNodeContentUI.transform, destination,moveLocal:true));
                    mRobotUpgradeNodeContentUI.DisplayUpgradeNode(null);
                }
                return;
            }
            if (mRobotUpgradeNodeContentUI.UnActivated)
            {
                StartCoroutine(UIUtils.TransitionUIElement((RectTransform)this.mRobotUpgradeNodeContentUI.transform, contentLocation));
            }
            mRobotUpgradeNodeContentUI.DisplayUpgradeNode(robotUpgradeNode);
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

            SerializedRobotUpgradeNodeNetwork serializedRobotUpgradeNodeNetwork = RobotUpgradeUtils.ToSerializedNetwork(nodeNetwork);
            string json = JsonConvert.SerializeObject(serializedRobotUpgradeNodeNetwork);
            File.WriteAllText(path, json);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //DisplayNodeContent(null);
        }
    }
}
