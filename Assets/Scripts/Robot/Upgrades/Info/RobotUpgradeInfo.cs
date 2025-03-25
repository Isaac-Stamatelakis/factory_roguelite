using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Robot.Upgrades.Info
{
    public interface IAmountFormatter
    {
        
    }
    public interface IDiscreteUpgradeAmountFormatter : IAmountFormatter
    {
        public string Format(int value);
    }

    public interface IContinousUpgradeAmountFormatter : IAmountFormatter
    {
        public string Format(float upgrade);
    }
    internal abstract class RobotUpgradeInfo
    {
        public abstract  List<TMP_Dropdown.OptionData> GetDropDownOptions();
        public abstract string GetDescription(int upgrade);
        public abstract string GetTitle(int upgrade);
        public abstract List<string> GetDefaultCosts();
        public abstract IAmountFormatter GetAmountFormatter(int upgrade);
        public abstract IAmountFormatter GetEnergyCostFormatter(int upgrade);
        public RobotStatLoadOut GetRobotStatLoadOut()
        {
            RobotStatLoadOut loadOut = new RobotStatLoadOut(new Dictionary<int, float>(), new Dictionary<int, int>());
            VerifyStatLoadOut(loadOut);
            return loadOut;
        }

        public void VerifyStatLoadOut(RobotStatLoadOut robotStatLoadOut)
        {
            robotStatLoadOut.DiscreteValues ??= new Dictionary<int, int>();
            robotStatLoadOut.ContinuousValues ??= new Dictionary<int, float>();
            List<int> continuousUpgrades = GetContinuousUpgrades();
            foreach (int continuousUpgrade in continuousUpgrades)
            {
                robotStatLoadOut.ContinuousValues.TryAdd(continuousUpgrade, 0);
                if (float.IsNaN(robotStatLoadOut.ContinuousValues[continuousUpgrade]))
                {
                    robotStatLoadOut.ContinuousValues[continuousUpgrade] = 0;
                }
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
        public abstract List<int> GetConstantUpgrades();
        public abstract List<int> GetAllUpgrades();
    }
}