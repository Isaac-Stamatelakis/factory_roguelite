using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace Robot.Upgrades.Info.Instances
{
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

        public override string GetTitle(int upgrade)
        {
            return ((ConduitSlicerUpgrade)upgrade).ToString();
        }
        

        public override List<int> GetContinuousUpgrades()
        {
            return new List<int>();
        }

        public override List<int> GetAllUpgrades()
        {
            return System.Enum.GetValues(typeof(ConduitSlicerUpgrade)).Cast<int>().ToList();
        }
    }
}