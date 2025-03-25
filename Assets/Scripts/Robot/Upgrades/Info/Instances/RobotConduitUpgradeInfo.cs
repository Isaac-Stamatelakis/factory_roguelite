using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace Robot.Upgrades.Info.Instances
{
    internal class RobotConduitUpgradeInfo : RobotUpgradeInfo
    {
        public const ulong COST_PER_HIT = 32;
        public override List<TMP_Dropdown.OptionData> GetDropDownOptions()
        {
            return GlobalHelper.EnumToDropDown<ConduitSlicerUpgrade>();
        }

        public override string GetDescription(int upgrade)
        {
            ConduitSlicerUpgrade robotDrillUpgrade = (ConduitSlicerUpgrade)upgrade;
            return robotDrillUpgrade switch
            {
                ConduitSlicerUpgrade.VeinMine => "Unlocks vein mine conduit breaking",
                ConduitSlicerUpgrade.Item_Magnet => "Teleports destroyed conduits into robot inventory",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public override string GetTitle(int upgrade)
        {
            return ((ConduitSlicerUpgrade)upgrade).ToString();
        }

        public override List<string> GetDefaultCosts()
        {
            return new List<string>
            {
                $"Requires {COST_PER_HIT}J/hit"
            };
        }

        public override IAmountFormatter GetAmountFormatter(int upgrade)
        {
            ConduitSlicerUpgrade conduitSlicerUpgrade = (ConduitSlicerUpgrade)upgrade;
            switch (conduitSlicerUpgrade)
            {
                case ConduitSlicerUpgrade.VeinMine:
                    return new VeinMineUpgradeFormatter();
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
            return System.Enum.GetValues(typeof(ConduitSlicerUpgrade)).Cast<int>().ToList();
        }
    }
}