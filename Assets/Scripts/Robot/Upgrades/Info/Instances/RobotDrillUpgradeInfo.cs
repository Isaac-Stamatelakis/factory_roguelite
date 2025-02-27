using System;
using System.Collections.Generic;
using System.Linq;
using TileEntity;
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

        public override IAmountFormatter GetAmountFormatter(int upgrade)
        {
            RobotDrillUpgrade robotDrillUpgrade = (RobotDrillUpgrade)upgrade;
            switch (robotDrillUpgrade)
            {
                case RobotDrillUpgrade.Speed:
                    return new RobotDrillSpeedUpgradeFormatter();
                case RobotDrillUpgrade.Fortune:
                    break;
                case RobotDrillUpgrade.MultiBreak:
                    break;
                case RobotDrillUpgrade.VeinMine:
                    return new VeinMineUpgradeFormatter();
                case RobotDrillUpgrade.Tier:
                    return new RobotDrillTierUpgradeFormatter();
            }
            return null;
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

    public class RobotDrillTierUpgradeFormatter : IDiscreteUpgradeAmountFormatter
    {
        public string Format(int value)
        {
            Tier tier = (Tier)value;
            return tier.ToString();
        }
    }

    public class VeinMineUpgradeFormatter : IContinousUpgradeAmountFormatter
    {
        public string Format(float upgrade)
        {
            int veinMinePower = RobotUpgradeUtils.GetVeinMinePower(upgrade);
            return veinMinePower <= 1 ? "Off" : veinMinePower.ToString();
        }
    }

    public class RobotDrillSpeedUpgradeFormatter : IContinousUpgradeAmountFormatter
    {
        public string Format(float upgrade)
        {
            return $"{upgrade:P2}";
        }
    }
}