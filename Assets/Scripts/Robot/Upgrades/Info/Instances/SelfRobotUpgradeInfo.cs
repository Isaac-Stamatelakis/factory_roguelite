using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace Robot.Upgrades.Info.Instances
{
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

        public override string GetTitle(int upgrade)
        {
            return ((RobotUpgrade)upgrade).ToString();
        }

        public override IAmountFormatter GetAmountFormatter(int upgrade)
        {
            RobotUpgrade robotUpgrade = (RobotUpgrade)upgrade;
            switch (robotUpgrade)
            {
                case RobotUpgrade.Speed:
                    break;
                case RobotUpgrade.JumpHeight:
                    break;
                case RobotUpgrade.BonusJump:
                    break;
                case RobotUpgrade.RocketBoots:
                    break;
                case RobotUpgrade.Flight:
                    break;
                case RobotUpgrade.Reach:
                    break;
                case RobotUpgrade.Dash:
                    break;
                case RobotUpgrade.Hover:
                    break;
                case RobotUpgrade.Teleport:
                    break;
                case RobotUpgrade.Light:
                    break;
                case RobotUpgrade.NightVision:
                    break;
            }

            return null;
        }

        public override List<int> GetContinuousUpgrades()
        {
            return new List<int>
            {
                (int)RobotUpgrade.Speed,
                (int)RobotUpgrade.JumpHeight,
                (int)RobotUpgrade.Reach
            };
        }

        public override List<int> GetConstantUpgrades()
        {
            return new List<int>();
        }

        public override List<int> GetAllUpgrades()
        {
            return System.Enum.GetValues(typeof(RobotUpgrade)).Cast<int>().ToList();
        }
    }

}