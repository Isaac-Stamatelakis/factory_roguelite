using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Player.Tool;
using TMPro;
using UI.NodeNetwork;
using UnityEngine;
using WorldModule;

namespace Robot.Upgrades {
    public static class RobotUpgradeUtils
    {
        public static RobotUpgradeNodeNetwork DeserializeRobotNodeNetwork(byte[] bytes)
        {
            string json = WorldLoadUtils.DecompressString(bytes);
            if (json == null) return null;
            try
            {
                return JsonConvert.DeserializeObject<RobotUpgradeNodeNetwork>(json);
            }
            catch (JsonSerializationException e)
            {
                Debug.LogWarning(e);
                return null;
            }
            
        } 
        public static int GetNextUpgradeId(RobotUpgradeNodeNetwork robotUpgradeNodeNetwork)
        {
            List<RobotUpgradeNode> nodes = robotUpgradeNodeNetwork.GetNodes();
            if (nodes == null || nodes.Count == 0) return 0;
            int largest = 0;
            foreach (RobotUpgradeNode node in nodes)
            {
                int id = node.GetId();
                if (id > largest) largest = id;
            }

            return largest + 1;
        }
        
        public static uint GetRequireAmountMultiplier(RobotUpgradeNode robotUpgradeNode)
        {
            if ((robotUpgradeNode.InstanceData?.Amount ?? 0) == 0) return 1;
            int amount = robotUpgradeNode.InstanceData.Amount;
            return (uint)(Mathf.Pow(robotUpgradeNode.NodeData.CostMultiplier, amount));
        }

        
    }

    internal static class RobotUpgradeInfoFactory
    {
        public static RobotUpgradeInfo GetRobotUpgradeInfo(RobotUpgradeType robotUpgradeType, int subType)
        {
            switch (robotUpgradeType)
            {
                case RobotUpgradeType.Tool:
                    return GetRobotToolUpgradeInfo((RobotToolType)subType);
                case RobotUpgradeType.Robot:
                    return new SelfRobotUpgradeInfo();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static RobotUpgradeInfo GetRobotToolUpgradeInfo(RobotToolType robotUpgradeType)
        {
            switch (robotUpgradeType)
            {
                case RobotToolType.LaserDrill:
                    return new RobotDrillUpgradeInfo();
                case RobotToolType.ConduitSlicers:
                    return new RobotConduitUpgradeInfo();
                case RobotToolType.LaserGun:
                case RobotToolType.Buildinator:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(robotUpgradeType), robotUpgradeType, null);
            }
        }
    }

    internal abstract class RobotUpgradeInfo
    {
        public abstract  List<TMP_Dropdown.OptionData> GetDropDownOptions();
        public abstract string GetDescription(int upgrade);
    }

    internal enum RobotUpgrade
    {
        Speed = 0,
        JumpHeight = 1,
        BonusJump = 2,
        RocketBoots = 3,
        Flight = 4,
        Reach = 5,
        Dash = 6,
        Hover = 7,
        Teleport = 8,
        
    }

    internal class SelfRobotUpgradeInfo : RobotUpgradeInfo
    {
        public override List<TMP_Dropdown.OptionData> GetDropDownOptions()
        {
            return GlobalHelper.EnumToDropDown<RobotUpgrade>();
        }

        public override string GetDescription(int upgrade)
        {
            RobotUpgrade robotUpgrade = (RobotUpgrade)upgrade;
            switch (robotUpgrade)
            {
                case RobotUpgrade.Speed:
                    return "Increases robot move speed";
                case RobotUpgrade.JumpHeight:
                    return "Increases robot jump height";
                case RobotUpgrade.BonusJump:
                    return "Grants bonus jumps in the air to robot";
                case RobotUpgrade.RocketBoots:
                    return "Grants bonus jumps in the air to robot";
                case RobotUpgrade.Flight:
                    return "Grants flight";
                case RobotUpgrade.Reach:
                case RobotUpgrade.Dash:
                case RobotUpgrade.Hover:
                case RobotUpgrade.Teleport:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    internal enum RobotDrillUpgrade
    {
        Speed = 0,
        Fortune = 1,
        MultiBreak = 2,
        VeinMine = 3,
    }
    
    internal class RobotDrillUpgradeInfo : RobotUpgradeInfo
    {
        public override List<TMP_Dropdown.OptionData> GetDropDownOptions()
        {
            return GlobalHelper.EnumToDropDown<RobotDrillUpgrade>();
        }

        public override string GetDescription(int upgrade)
        {
            RobotDrillUpgrade robotDrillUpgrade = (RobotDrillUpgrade)upgrade;
            switch (robotDrillUpgrade)
            {
                case RobotDrillUpgrade.Speed:
                    return "Increases mining speed";
                case RobotDrillUpgrade.Fortune:
                    return "Higher chance of drops";
                case RobotDrillUpgrade.MultiBreak:
                    return "Unlocks higher break sizes";
                case RobotDrillUpgrade.VeinMine:
                    return "Unlocks vein mein";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    internal enum ConduitSlicerUpgrade
    {
        VeinMine = 0,
    }
    
    internal class RobotConduitUpgradeInfo : RobotUpgradeInfo
    {
        public override List<TMP_Dropdown.OptionData> GetDropDownOptions()
        {
            return GlobalHelper.EnumToDropDown<ConduitSlicerUpgrade>();
        }

        public override string GetDescription(int upgrade)
        {
            ConduitSlicerUpgrade robotDrillUpgrade = (ConduitSlicerUpgrade)upgrade;
            switch (robotDrillUpgrade)
            {
                case ConduitSlicerUpgrade.VeinMine:
                    return "Unlocks vein mine conduit breaking";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
    
    public enum RobotUpgradeType
    {
        Tool = 0,
        Robot = 1,
        
    }
    

    public class RobotUpgradeNodeData
    {
        public int Id;
        public int UpgradeType;
        public int UpgradeAmount = 1;
        public float CostMultiplier = 1;
        public List<SerializedItemSlot> Cost = new List<SerializedItemSlot>();
        public List<int> PreReqs = new List<int>();
        public float X;
        public float Y;
        public string IconItemId;

        public RobotUpgradeNodeData(int id)
        {
            Id = id;
        }
    }
    public class RobotUpgradeNode : INode
    {
        public RobotUpgradeNodeData NodeData;
        public RobotUpgradeData InstanceData;
        public Vector3 GetPosition()
        {
            return new Vector3(NodeData.X, NodeData.Y);
        }

        public void SetPosition(Vector3 pos)
        {
            NodeData.X = pos.x;
            NodeData.Y = pos.y;
        }

        public int GetId()
        {
            return NodeData.Id;
        }

        public List<int> GetPrerequisites()
        {
            return NodeData.PreReqs;
        }

        public bool IsCompleted()
        {
            return (InstanceData?.Amount ?? 0) >= NodeData.UpgradeAmount;
        }

        public RobotUpgradeNode(RobotUpgradeNodeData nodeData, RobotUpgradeData instanceData)
        {
            NodeData = nodeData;
            InstanceData = instanceData;
        }
    }

    public class RobotUpgradeNodeNetwork : INodeNetwork<RobotUpgradeNode>
    {
        public RobotUpgradeType Type;
        public int SubType;
        public List<RobotUpgradeNode> UpgradeNodes;
        public List<RobotUpgradeNode> GetNodes()
        {
            return UpgradeNodes;
        }

        public RobotUpgradeNodeNetwork(RobotUpgradeType type, int subType, List<RobotUpgradeNode> upgradeNodes)
        {
            Type = type;
            SubType = subType;
            UpgradeNodes = upgradeNodes;
        }
    }
    public class RobotUpgradeData
    {
        public int Id;
        public int Amount;

        public RobotUpgradeData(int id, int amount)
        {
            Id = id;
            Amount = amount;
        }
    }
    
}

