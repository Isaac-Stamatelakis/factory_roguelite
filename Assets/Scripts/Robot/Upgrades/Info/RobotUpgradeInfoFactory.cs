using System;
using Player.Tool;
using Robot.Upgrades.Info.Instances;

namespace Robot.Upgrades.Info
{
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
}