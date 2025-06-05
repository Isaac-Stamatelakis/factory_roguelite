using System;
using System.Collections.Generic;
using Player.Tool;
using Robot.Upgrades.Info;
using UnityEngine;

namespace Robot.Upgrades.LoadOut
{
    public class RobotUpgradeLoadOut
    {
        public RobotStatLoadOutCollection SelfLoadOuts;
        public Dictionary<RobotToolType, RobotStatLoadOutCollection> ToolLoadOuts;

        public RobotUpgradeLoadOut(RobotStatLoadOutCollection selfLoadOuts, Dictionary<RobotToolType, RobotStatLoadOutCollection> toolLoadOuts)
        {
            SelfLoadOuts = selfLoadOuts;
            ToolLoadOuts = toolLoadOuts;
        }

        public RobotStatLoadOutCollection GetToolLoadOut(RobotToolType toolType)
        {
            return ToolLoadOuts.GetValueOrDefault(toolType);
        }

        public RobotStatLoadOutCollection GetCollection(RobotUpgradeType upgradeType, int subType)
        {
            switch (upgradeType)
            {
                case RobotUpgradeType.Tool:
                    return GetToolLoadOut((RobotToolType)subType);
                case RobotUpgradeType.Robot:
                    return SelfLoadOuts;
                default:
                    throw new ArgumentOutOfRangeException(nameof(upgradeType), upgradeType, null);
            }
        }
    }

    public class RobotStatLoadOutCollection
    {
        public int Current;
        public List<RobotStatLoadOut> LoadOuts;

        public RobotStatLoadOutCollection(int current, List<RobotStatLoadOut> loadOuts)
        {
            Current = current;
            LoadOuts = loadOuts;
        }

        public RobotStatLoadOut GetLoadOut(int index)
        {
            if (index < 0 || index >= LoadOuts.Count) return null;
            return LoadOuts[index];
        }

        public RobotStatLoadOut GetCurrent()
        {
            return GetLoadOut(Current);
        }

        public void IncrementCurrent(int amount)
        {
            Current = (Current + amount) % LoadOuts.Count;
            if (Current < 0)
            {
                Current += LoadOuts.Count;
            }
        }

        public void OverrideUpgradeCount(RobotUpgradeType robotUpgrade, int subType)
        {
            var info = RobotUpgradeInfoFactory.GetRobotUpgradeInfo(robotUpgrade,subType);
            Array upgrades;
            switch (robotUpgrade)
            {
                case RobotUpgradeType.Tool:
                    upgrades =  Enum.GetValues(typeof(RobotUpgrade));
                    break;
                case RobotUpgradeType.Robot:
                    RobotToolType toolType = (RobotToolType)subType;
                    switch (toolType)
                    {
                        case RobotToolType.LaserGun:
                            upgrades = Enum.GetValues(typeof(LaserGunUpgrade));
                            break;
                        case RobotToolType.LaserDrill:
                            upgrades = Enum.GetValues(typeof(RobotDrillUpgrade));
                            break;
                        case RobotToolType.ConduitSlicers:
                            upgrades = Enum.GetValues(typeof(ConduitSlicerUpgrade));
                            break;
                        case RobotToolType.Buildinator:
                            upgrades = Enum.GetValues(typeof(BuildinatorUpgrade));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(robotUpgrade), robotUpgrade, null);
            }

            List<int> continousUpgrades = info.GetContinuousUpgrades();
            foreach (var upgrade in upgrades)
            {
                int value = (int)upgrade;
                foreach (RobotStatLoadOut robotStatLoadOut in LoadOuts)
                {
                    
                }
            }
            
        }
    }
    
}
