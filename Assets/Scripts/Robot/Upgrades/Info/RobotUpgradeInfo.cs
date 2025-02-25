using System.Collections.Generic;
using TMPro;

namespace Robot.Upgrades.Info
{
    internal abstract class RobotUpgradeInfo
    {
        public abstract  List<TMP_Dropdown.OptionData> GetDropDownOptions();
        public abstract string GetDescription(int upgrade);
        public abstract string GetTitle(int upgrade);

        public RobotStatLoadOut GetRobotStatLoadOut()
        {
            RobotStatLoadOut loadOut = new RobotStatLoadOut(new Dictionary<int, float>(), new Dictionary<int, int>());
            VerifyStatLoadOut(loadOut);
            return loadOut;
        }

        public void VerifyStatLoadOut(RobotStatLoadOut robotStatLoadOut)
        {
            List<int> continuousUpgrades = GetContinuousUpgrades();
            foreach (int continuousUpgrade in continuousUpgrades)
            {
                robotStatLoadOut.ContinuousValues.TryAdd(continuousUpgrade, 0);

                if (robotStatLoadOut.DiscreteValues.ContainsKey(continuousUpgrade))
                {
                    robotStatLoadOut.DiscreteValues.Remove(continuousUpgrade);
                }
            }

            List<int> allUpgrades = GetAllUpgrades();
            foreach (int upgrade in allUpgrades)
            {
                if (continuousUpgrades.Contains(upgrade)) continue;
                robotStatLoadOut.DiscreteValues.TryAdd(upgrade, 0);
                if (robotStatLoadOut.ContinuousValues.ContainsKey(upgrade))
                {
                    robotStatLoadOut.ContinuousValues.Remove(upgrade);
                }
            }

        }
        public abstract List<int> GetContinuousUpgrades();
        public abstract List<int> GetUnEditableUpg
        public abstract List<int> GetAllUpgrades();
    }
}