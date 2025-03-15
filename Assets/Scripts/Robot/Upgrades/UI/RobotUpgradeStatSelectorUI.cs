using System;
using System.Collections.Generic;
using Robot.Upgrades.Info;
using Robot.Upgrades.LoadOut;
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
        
        public float GetCountinuousValue(int upgrade)
        {
            return ContinuousValues.GetValueOrDefault(upgrade);
        }

        public int GetDiscreteValue(int upgrade)
        {
            return DiscreteValues.GetValueOrDefault(upgrade);
        }
    }
    
    public class RobotUpgradeStatSelectorUI : MonoBehaviour
    {
        [SerializeField] private VerticalLayoutGroup mList;
        [SerializeField] private FormattedSlider mNumSliderPrefab;
        [SerializeField] private FormattedSlider mBoolSliderPrefab;
        [SerializeField] private Transform presetList;
        private Button[] presetSelectButtons;
        private Color buttonColor;
        private Color highlightButtonColor = Color.yellow;

        private RobotStatLoadOutCollection robotStatLoadOutCollection;
        private Dictionary<int, int> statUpgradeDict;
        private RobotUpgradeInfo robotUpgradeInfo;
        
        
        public void Initialize()
        {
            presetSelectButtons = presetList.GetComponentsInChildren<Button>();
            if (presetSelectButtons.Length == 0) return;
            buttonColor = presetSelectButtons[0].GetComponent<Image>().color;
            for (var i = 0; i < presetSelectButtons.Length; i++)
            {
                var button = presetSelectButtons[i];
                var buttonIndex = i; // This is suggested by rider, think it prevents i from changing in each lambda
                button.onClick.AddListener(() =>
                {
                    SelectPreset(buttonIndex);
                });
            }
        }

        private void SelectPreset(int index)
        {
            if (robotStatLoadOutCollection == null || robotStatLoadOutCollection.Current == index) return;
            presetSelectButtons[robotStatLoadOutCollection.Current].GetComponent<Image>().color = buttonColor;
            robotStatLoadOutCollection.Current = index;
            presetSelectButtons[robotStatLoadOutCollection.Current].GetComponent<Image>().color = highlightButtonColor;
            DisplayCurrentStatLoadOut();
        }
        

        internal void Display(RobotStatLoadOutCollection statLoadOutCollection, Dictionary<int, int> statUpgradeDict, RobotUpgradeInfo upgradeInfo)
        {
            Initialize();
            this.robotStatLoadOutCollection = statLoadOutCollection;
            this.statUpgradeDict = statUpgradeDict;
            this.robotUpgradeInfo = upgradeInfo;
            presetSelectButtons[robotStatLoadOutCollection.Current].GetComponent<Image>().color = highlightButtonColor;
            DisplayCurrentStatLoadOut();
        }

        private void DisplayCurrentStatLoadOut()
        {
            if (robotStatLoadOutCollection.GetCurrent() == null)
            {
                List<RobotStatLoadOut> robotStatLoadOuts = robotStatLoadOutCollection.LoadOuts;
                while (robotStatLoadOuts.Count <= robotStatLoadOutCollection.Current)
                {
                    robotStatLoadOuts.Add(new RobotStatLoadOut(new Dictionary<int, float>(),new Dictionary<int, int>()));
                }
            }
            RobotStatLoadOut statLoadOut = this.robotStatLoadOutCollection.GetCurrent();
            robotUpgradeInfo.VerifyStatLoadOut(statLoadOut);
            GlobalHelper.deleteAllChildren(mList.transform);
            List<int> upgrades = robotUpgradeInfo.GetAllUpgrades();
            List<int> constantUpgrades = robotUpgradeInfo.GetConstantUpgrades();
            upgrades.Sort();
            foreach (int upgrade in upgrades)
            {
                if (!statUpgradeDict.TryGetValue(upgrade, out var maxValue)) continue;
                if (maxValue == 0) continue;
                string title = robotUpgradeInfo.GetTitle(upgrade);
                bool isConstant = constantUpgrades.Contains(upgrade);
                FormattedSlider slider = GetSlider(statLoadOut, title, upgrade, maxValue,isConstant, robotUpgradeInfo);
                if (!slider)
                {
                    continue;
                }
                if (isConstant) slider.GetComponentInChildren<Scrollbar>().gameObject.SetActive(false);
                
            }
        }

        private FormattedSlider GetSlider(RobotStatLoadOut statLoadOut, string title, int upgrade, int maxValue, bool isConstant, RobotUpgradeInfo upgradeInfo)
        {
            if (statLoadOut.DiscreteValues.TryGetValue(upgrade, out int intValue))
            {
                if (intValue < 0)
                {
                    statLoadOut.DiscreteValues[upgrade] = 0;
                }

                if (intValue > maxValue)
                {
                    statLoadOut.DiscreteValues[upgrade] = maxValue;
                }
      
                void OnValueChanged(int newValue)
                {
                    statLoadOut.DiscreteValues[upgrade] = newValue;
                };
                if (maxValue == 1 && !isConstant)
                {
                    FormattedSlider slider = Instantiate(mBoolSliderPrefab, mList.transform);
                    slider.DisplayBool(title,intValue,OnValueChanged);
                    return slider;
                }
                else
                {
                    FormattedSlider slider = Instantiate(mNumSliderPrefab, mList.transform);
                    slider.DisplayInteger(title,intValue,maxValue,OnValueChanged,upgradeInfo.GetAmountFormatter(upgrade) as IDiscreteUpgradeAmountFormatter);
                    return slider;
                }
            }

            if (statLoadOut.ContinuousValues.TryGetValue(upgrade, out float floatValue))
            {
                if (floatValue < 0)
                {
                    statLoadOut.ContinuousValues[upgrade] = 0;
                }

                if (floatValue > maxValue)
                {
                    statLoadOut.ContinuousValues[upgrade] = maxValue;
                }
                
                void OnValueChanged(float newValue)
                {
                    statLoadOut.ContinuousValues[upgrade] = newValue;
                }

                FormattedSlider slider = Instantiate(mNumSliderPrefab, mList.transform);
                slider.DisplayFloat(title,floatValue,maxValue,OnValueChanged,upgradeInfo.GetAmountFormatter(upgrade) as IContinousUpgradeAmountFormatter);
                return slider;
            }
            return null;
        }
    }
    
}
