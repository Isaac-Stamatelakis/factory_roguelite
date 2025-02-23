using System.Collections.Generic;
using UI.NodeNetwork;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Robot.Upgrades
{
    public class RobotUpgradeNetworkUI : NodeNetworkUI<RobotUpgradeNode, RobotUpgradeNodeNetwork>
    {
        [SerializeField] private RobotUpgradeNodeUI robotUpgradeNodeUIPrefab;
        public void Initialize(RobotUpgradeNodeNetwork robotUpgradeNodeNetwork)
        {
            nodeNetwork = robotUpgradeNodeNetwork;
            editController.gameObject.SetActive(SceneManager.GetActiveScene().name == "DevTools");
            editController.Initialize(this);
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
