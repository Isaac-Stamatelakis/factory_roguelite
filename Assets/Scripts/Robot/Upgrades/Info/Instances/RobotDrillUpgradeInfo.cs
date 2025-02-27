using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

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
            return robotDrillUpgrade switch
            {
                RobotDrillUpgrade.Speed => "Increases mining speed",
                RobotDrillUpgrade.Fortune => "Higher chance of drops",
                RobotDrillUpgrade.MultiBreak => "Unlocks higher break sizes",
                RobotDrillUpgrade.VeinMine => "Unlocks vein mein",
                RobotDrillUpgrade.Tier => "Increases mining tier",
                RobotDrillUpgrade.Item_Magnet => "Teleports destroyed items into robot inventory",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public override string GetTitle(int upgrade)
        {
            return ((RobotDrillUpgrade)upgrade).ToString();
        }
        
        public override List<int> GetContinuousUpgrades()
        {
            Debug.Log((int)RobotDrillUpgrade.VeinMine);
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