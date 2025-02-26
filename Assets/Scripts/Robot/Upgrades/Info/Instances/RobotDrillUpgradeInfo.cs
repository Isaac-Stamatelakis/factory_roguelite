using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace Robot.Upgrades.Info.Instances
{
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
                case RobotDrillUpgrade.Tier:
                    return "Increases mining tier";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string GetTitle(int upgrade)
        {
            return ((RobotDrillUpgrade)upgrade).ToString();
        }
        
        public override List<int> GetContinuousUpgrades()
        {
            return new List<int>
            {
                (int)RobotDrillUpgrade.Speed,
                (int)RobotDrillUpgrade.VeinMine
            };
        }

        public override List<int> GetConstantUpgrades()
        {
            return new List<int>
            {
                (int)RobotDrillUpgrade.Tier,
            };
        }

        public override List<int> GetAllUpgrades()
        {
            return System.Enum.GetValues(typeof(RobotDrillUpgrade)).Cast<int>().ToList();
        }
    }
}