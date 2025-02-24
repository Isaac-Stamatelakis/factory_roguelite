using System;
using System.Collections.Generic;
using UI.GeneralUIElements.Sliders;
using UnityEngine;
using UnityEngine.UI;

namespace Robot.Upgrades
{
    public class RobotStatLoadOut
    {
        public Dictionary<int, int> ContinuousValues;
        public Dictionary<int, float> DiscreteValues;

        public RobotStatLoadOut(Dictionary<int, int> continuousValues, Dictionary<int, float> discreteValues)
        {
            ContinuousValues = continuousValues;
            DiscreteValues = discreteValues;
        }
    }
    
    public class RobotUpgradeStatSelectorUI : MonoBehaviour
    {
        [SerializeField] private VerticalLayoutGroup mList;
        [SerializeField] private FormattedSlider mNumSliderPrefab;
        [SerializeField] private FormattedSlider mBoolSliderPrefab;
        
        internal void Display(RobotStatLoadOut statLoadOut, Dictionary<int, int> statUpgradeDict, RobotUpgradeInfo upgradeInfo)
        {
            GlobalHelper.deleteAllChildren(mList.transform);
            List<int> upgrades = new List<int>();
            foreach (var (id, value) in statLoadOut.ContinuousValues)
            {
                if (!statUpgradeDict.ContainsKey(id)) continue;
                upgrades.Add(id);
            }

            foreach (var (id, value) in statLoadOut.DiscreteValues)
            {
                if (!statUpgradeDict.ContainsKey(id)) continue;
                upgrades.Add(id);
            }
            upgrades.Sort();
            foreach (int upgrade in upgrades)
            {
                string title = upgradeInfo.GetTitle(upgrade);
                int maxValue = statUpgradeDict[upgrade];
                if (statLoadOut.ContinuousValues.TryGetValue(upgrade, out int intValue))
                {
                    void OnValueChanged(int newValue)
                    {
                        statLoadOut.ContinuousValues[upgrade] = newValue;
                    };
                    if (maxValue == 1)
                    {
                        FormattedSlider slider = Instantiate(mBoolSliderPrefab, mList.transform);
                        slider.DisplayBool(title,intValue,OnValueChanged);
                    }
                    else
                    {
                        FormattedSlider slider = Instantiate(mNumSliderPrefab, mList.transform);
                        slider.DisplayInteger(title,intValue,maxValue,0,OnValueChanged);
                    }
                }

                if (statLoadOut.DiscreteValues.TryGetValue(upgrade, out float floatValue))
                {
                    void OnValueChanged(float newValue)
                    {
                        statLoadOut.DiscreteValues[upgrade] = newValue;
                    }

                    FormattedSlider slider = Instantiate(mNumSliderPrefab, mList.transform);
                    slider.DisplayFloat(title,floatValue,maxValue,0,OnValueChanged);
                }
            }
        }
    }
}
