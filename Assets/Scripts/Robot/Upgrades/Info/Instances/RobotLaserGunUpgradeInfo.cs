using System;
using System.Collections.Generic;
using System.Linq;
using TileEntity;
using TMPro;
using UnityEngine;

namespace Robot.Upgrades.Info.Instances
{
    internal class RobotLaserGunUpgradeInfo : RobotUpgradeInfo
    {
        public const ulong COST_PER_LASER = 32;
        public const ulong COST_PER_EXPLOSION = 1024;
        public override List<TMP_Dropdown.OptionData> GetDropDownOptions()
        {
            return GlobalHelper.EnumToDropDown<LaserGunUpgrade>();
        }

        public override string GetDescription(int upgrade)
        {
            LaserGunUpgrade buildinatorUpgrade = (LaserGunUpgrade)upgrade;
            return buildinatorUpgrade switch
            {
                LaserGunUpgrade.FireRate => "Increases Fire Rate",
                LaserGunUpgrade.MultiShot => "Increases Lasers Fired",
                LaserGunUpgrade.AoE => "Unlocks AoE Explosions",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public override string GetTitle(int upgrade)
        {
            return ((LaserGunUpgrade)upgrade).ToString();
        }

        public override IAmountFormatter GetAmountFormatter(int upgrade)
        {
            return null;
        }

        public override IAmountFormatter GetEnergyCostFormatter(int upgrade)
        {
            return null;
        }

        public override List<int> GetContinuousUpgrades()
        {
            return new List<int>
            {
                (int)LaserGunUpgrade.FireRate
            };
        }

        public override List<int> GetConstantUpgrades()
        {
            return new List<int>();
        }

        public override List<int> GetAllUpgrades()
        {
            return System.Enum.GetValues(typeof(LaserGunUpgrade)).Cast<int>().ToList();
        }
        
        public override List<string> GetDefaultCosts()
        {
            return new List<string>
            {
                $"Requires: {COST_PER_LASER}J/laser",
                $"Requires: {COST_PER_EXPLOSION}J/explosion"
            };
        }
    }
}