using System;
using System.Collections.Generic;
using UI.GeneralUIElements.Sliders;
using UnityEngine;
using UnityEngine.UI;

namespace Robot.Upgrades
{
    public class RobotStatLoadOut
    {
        public Dictionary<int, float> ContinuousValues;
        public Dictionary<int, int> DiscreteValues;

        public RobotStatLoadOut(Dictionary<int, float> continuousValues, Dictionary<int, int> discreteValues)
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
            List<int> upgrades = upgradeInfo.GetAllUpgrades();
            upgrades.Sort();
            foreach (int upgrade in upgrades)
            {
                if (!statUpgradeDict.TryGetValue(upgrade, out var maxValue)) continue;

                string title = upgradeInfo.GetTitle(upgrade);
                if (statLoadOut.DiscreteValues.TryGetValue(upgrade, out int intValue))
                {
                    void OnValueChanged(int newValue)
                    {
                        statLoadOut.DiscreteValues[upgrade] = newValue;
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

                if (statLoadOut.ContinuousValues.TryGetValue(upgrade, out float floatValue))
                {
                    void OnValueChanged(float newValue)
                    {
                        statLoadOut.ContinuousValues[upgrade] = newValue;
                    }

                    FormattedSlider slider = Instantiate(mNumSliderPrefab, mList.transform);
                    slider.DisplayFloat(title,floatValue,maxValue,0,OnValueChanged);
                }
            }
        }
    }
}
