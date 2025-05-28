using System;
using System.Collections.Generic;
using Player;
using Robot.Upgrades.Info;
using Robot.Upgrades.LoadOut;
using Robot.Upgrades.Network;
using TMPro;
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
        [SerializeField] private TextMeshProUGUI mTitleText;
        [SerializeField] private VerticalLayoutGroup mList;
        [SerializeField] private FormattedSlider mNumSliderPrefab;
        [SerializeField] private Transform presetList;
        [SerializeField] private RobotUpgradeEnergyCostElement upgradeEnergyCostElement;
        private Button[] presetSelectButtons;
        private Color buttonColor;
        private Color highlightButtonColor = Color.green;

        private RobotStatLoadOutCollection robotStatLoadOutCollection;
        private Dictionary<int, int> statUpgradeDict;
        private RobotUpgradeInfo robotUpgradeInfo;
        private PlayerScript playerScript;
        private Action<int> onLoadOutChange;
        private Dictionary<int, Action> upgradeCallbackDict;
        
        private void Initialize(RobotUpgradeInfo upgradeInfo)
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
            
            string costText = upgradeInfo.GetDefaultCosts();
            upgradeEnergyCostElement.gameObject.SetActive(!string.IsNullOrEmpty(costText));
            upgradeEnergyCostElement.SetDisplayText(costText);
        }

        public void AddListener(Action callback, int upgrade)
        {
            upgradeCallbackDict[upgrade] = callback;
        }

        private void SelectPreset(int loadOut)
        {
            if (robotStatLoadOutCollection == null || robotStatLoadOutCollection.Current == loadOut) return;
            presetSelectButtons[robotStatLoadOutCollection.Current].GetComponent<Image>().color = buttonColor;
            robotStatLoadOutCollection.Current = loadOut;
            onLoadOutChange?.Invoke(loadOut);
            presetSelectButtons[robotStatLoadOutCollection.Current].GetComponent<Image>().color = highlightButtonColor;
            DisplayCurrentStatLoadOut();
        }

        internal bool Display(UpgradeDisplayData upgradeDisplayData)
        {
            bool error = false;
            if (upgradeDisplayData.StatLoadOutCollection == null)
            {
                error = true;
                Debug.LogWarning("Tried to display null stat load out");
            }

            SerializedRobotUpgradeNodeNetwork network = RobotUpgradeUtils.DeserializeRobotNodeNetwork(upgradeDisplayData.UpgradePath);
            if (network == null)
            {
                error = true;
                Debug.LogWarning("Tried to display null network");
            }

            if (error)
            {
                GameObject.Destroy(gameObject);
                return false;
            }
            
            Dictionary<int, int> upgradeDict = RobotUpgradeUtils.GetAmountOfUpgrades(network.NodeData, upgradeDisplayData.UpgradeData);
            Display(upgradeDisplayData.StatLoadOutCollection,upgradeDict,upgradeDisplayData.RobotUpgradeInfo,upgradeDisplayData.OnLoadOutChange,upgradeDisplayData.UpgradeChangeCallbacks,upgradeDisplayData.Title);
            return true;
        }

        private void Display(RobotStatLoadOutCollection statLoadOutCollection, Dictionary<int, int> statUpgradeDict, RobotUpgradeInfo upgradeInfo, Action<int> onLoadOutChange, Dictionary<int,Action> upgradeCallbacks, string title)
        {
            mTitleText.text = title;
            this.onLoadOutChange = onLoadOutChange;
            Initialize(upgradeInfo);
            this.robotStatLoadOutCollection = statLoadOutCollection;
            this.statUpgradeDict = statUpgradeDict;
            this.robotUpgradeInfo = upgradeInfo;
            presetSelectButtons[robotStatLoadOutCollection.Current].GetComponent<Image>().color = highlightButtonColor;
            this.upgradeCallbackDict = upgradeCallbacks;
            upgradeCallbackDict ??= new Dictionary<int, Action>();
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
            GlobalHelper.DeleteAllChildren(mList.transform);
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
            FormattedSlider slider = Instantiate(mNumSliderPrefab, mList.transform);
            RobotUpgradeEnergyCostElement energyCostElement = slider.GetComponentInChildren<RobotUpgradeEnergyCostElement>();
            
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
      
                IDiscreteUpgradeAmountFormatter energyFormatter = upgradeInfo.GetEnergyCostFormatter(upgrade) as IDiscreteUpgradeAmountFormatter;
                energyCostElement.gameObject.SetActive(energyFormatter != null);

                void SetEnergyCostText(int input)
                {
                    if (energyFormatter != null)
                    {
                        energyCostElement.SetDisplayText(energyFormatter.Format(input));
                    }
                }
                SetEnergyCostText(intValue);
                void OnValueChanged(int newValue)
                {
                    statLoadOut.DiscreteValues[upgrade] = newValue;
                    SetEnergyCostText(newValue);
                    if (upgradeCallbackDict.ContainsKey(upgrade))
                    {
                        upgradeCallbackDict[upgrade].Invoke();
                    }
                };
                if (maxValue == 1 && !isConstant)
                {
                    
                    slider.DisplayBool(title,intValue,OnValueChanged);
                }
                else
                {
                    slider.DisplayInteger(title,intValue,maxValue,OnValueChanged,upgradeInfo.GetAmountFormatter(upgrade) as IDiscreteUpgradeAmountFormatter);
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
                IContinousUpgradeAmountFormatter energyFormatter = upgradeInfo.GetEnergyCostFormatter(upgrade) as IContinousUpgradeAmountFormatter;
                energyCostElement.gameObject.SetActive(energyFormatter != null);
                void SetEnergyCostText(float input)
                {
                    if (energyFormatter != null)
                    {
                        energyCostElement.SetDisplayText(energyFormatter.Format(input));
                    }
                }

                SetEnergyCostText(floatValue);
                void OnValueChanged(float newValue)
                {
                    statLoadOut.ContinuousValues[upgrade] = newValue;
                    SetEnergyCostText(newValue);
                    if (upgradeCallbackDict.ContainsKey(upgrade))
                    {
                        upgradeCallbackDict[upgrade].Invoke();
                    }
                }
                slider.DisplayFloat(title,floatValue,maxValue,OnValueChanged,upgradeInfo.GetAmountFormatter(upgrade) as IContinousUpgradeAmountFormatter);
            }
            
            return slider;
        }
        
        internal struct UpgradeDisplayData
        {
            public string UpgradePath;
            public RobotStatLoadOutCollection StatLoadOutCollection;
            public List<RobotUpgradeData> UpgradeData;
            public RobotUpgradeInfo RobotUpgradeInfo;
            public Action<int> OnLoadOutChange;
            public Dictionary<int, Action> UpgradeChangeCallbacks;
            public string Title;

            public UpgradeDisplayData(string upgradePath, RobotStatLoadOutCollection statLoadOutCollection, List<RobotUpgradeData> upgradeData, 
                RobotUpgradeInfo robotUpgradeInfo, Action<int> onLoadOutChange, Dictionary<int, Action> upgradeChangeCallbacks, string title)
            {
                UpgradePath = upgradePath;
                StatLoadOutCollection = statLoadOutCollection;
                UpgradeData = upgradeData;
                RobotUpgradeInfo = robotUpgradeInfo;
                OnLoadOutChange = onLoadOutChange;
                UpgradeChangeCallbacks = upgradeChangeCallbacks;
                Title = title;
            }
        }
    }
    
    
}
