using System.Collections.Generic;
using DevTools;
using Robot.Upgrades.Network;
using UI.NodeNetwork;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Robot.Upgrades
{
    public class RobotUpgradeNetworkUI : NodeNetworkUI<RobotUpgradeNode, RobotUpgradeNodeNetwork>
    {
        [SerializeField] private RobotUpgradeNodeUI robotUpgradeNodeUIPrefab;
        private RobotUpgradeUI robotUpgradeUI;
        public RobotUpgradeUI RobotUpgradeUI => robotUpgradeUI;
        public void Initialize(RobotUpgradeUI robotUpgradeUI, RobotUpgradeNodeNetwork robotUpgradeNodeNetwork)
        {
            this.robotUpgradeUI = robotUpgradeUI;
            nodeNetwork = robotUpgradeNodeNetwork;
            bool inDevTools = DevToolUtils.OnDevToolScene;
            editController.gameObject.SetActive(inDevTools);
            editController.Initialize(this);
            lockHorizontalMovement = true;
            lockZoom = true;
            if (!inDevTools)
            {
                SetViewBounds();
            }
            
            Display();
        }
        protected override INodeUI GenerateNode(RobotUpgradeNode node)
        {
            RobotUpgradeNodeUI robotUpgradeNodeUI = GameObject.Instantiate(robotUpgradeNodeUIPrefab);
            robotUpgradeNodeUI.Initialize(node,this);
            RectTransform nodeRectTransform = (RectTransform)robotUpgradeNodeUI.transform;
            robotUpgradeNodeUI.transform.SetParent(nodeContainer,false); // Even though rider suggests changing this, it is wrong to
            return robotUpgradeNodeUI;
        }

        public override bool ShowAllComplete()
        {
            return false;
        }

        public override void OnDeleteSelectedNode()
        {
            robotUpgradeUI.DisplayNodeContent(null);
        }

        public override RobotUpgradeNode LookUpNode(int id)
        {
            foreach (RobotUpgradeNode node in nodeNetwork.UpgradeNodes)
            {
                if (node.GetId() == id) return node;
            }

            return null;
        }


        public override void PlaceNewNode(Vector2 position)
        {
            int id = RobotUpgradeUtils.GetNextUpgradeId(nodeNetwork);
            RobotUpgradeNodeData nodeData = new RobotUpgradeNodeData(id);
            RobotUpgradeData upgradeData = new RobotUpgradeData(id, 0);
            RobotUpgradeNode robotUpgradeNode = new RobotUpgradeNode(nodeData, upgradeData);
            robotUpgradeNode.SetPosition(position);
            nodeNetwork.UpgradeNodes ??= new List<RobotUpgradeNode>();
            nodeNetwork.UpgradeNodes.Add(robotUpgradeNode);
        }

        public override GameObject GenerateNewNodeObject()
        {
            return GameObject.Instantiate(robotUpgradeNodeUIPrefab).gameObject;
        }
    }
}
