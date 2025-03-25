using System;
using System.Collections.Generic;
using System.Linq;
using TileEntity;
using TMPro;
using UnityEngine;

namespace Robot.Upgrades.Info.Instances
{
    internal class BuildinatorUpgradeInfo : RobotUpgradeInfo
    {
        public override List<TMP_Dropdown.OptionData> GetDropDownOptions()
        {
            return GlobalHelper.EnumToDropDown<BuildinatorUpgrade>();
        }

        public override string GetDescription(int upgrade)
        {
            BuildinatorUpgrade buildinatorUpgrade = (BuildinatorUpgrade)upgrade;
            return buildinatorUpgrade switch
            {
                BuildinatorUpgrade.MultiHit => "Effects multiple targets",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public override string GetTitle(int upgrade)
        {
            return ((BuildinatorUpgrade)upgrade).ToString();
        }

        public override IAmountFormatter GetAmountFormatter(int upgrade)
        {
            BuildinatorUpgrade buildinatorUpgrade = (BuildinatorUpgrade)upgrade;
            switch (buildinatorUpgrade)
            {
                case BuildinatorUpgrade.MultiHit:
                    return new RobotDrillMultiBreakUpgradeFormatter();
            }
            return null;
        }

        public override IAmountFormatter GetEnergyCostFormatter(int upgrade)
        {
            return null;
        }

        public override List<int> GetContinuousUpgrades()
        {
            return new List<int>();
        }

        public override List<int> GetConstantUpgrades()
        {
            return new List<int>();
        }

        public override List<int> GetAllUpgrades()
        {
            return System.Enum.GetValues(typeof(BuildinatorUpgrade)).Cast<int>().ToList();
        }
    }
}