using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Player.Tool;
using Robot.Upgrades;
using Robot.Upgrades.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WorldModule;


namespace DevTools.Upgrades
{
    internal class DevToolNewUpgradePopUp : MonoBehaviour
    {
        [SerializeField] private TMP_InputField mInputField;
        [SerializeField] private Button mBackButton;
        [SerializeField] private Button mCreateButton;
        [SerializeField] private TMP_Dropdown mTypeDropDownMenu;
        [SerializeField] private TMP_Dropdown mSubTypeDropDownMenu;
        private DevToolUpgradeSelector upgradeSelector;
        internal void Initialize(DevToolUpgradeSelector upgradeSelector)
        {
            this.upgradeSelector = upgradeSelector;
            mBackButton.onClick.AddListener(() =>
            {
                GameObject.Destroy(gameObject);
            });
            mCreateButton.onClick.AddListener(CreateClick);
            mTypeDropDownMenu.onValueChanged.AddListener((int value) =>
            {
                DisplayTypeDropDownMenu();
            });
            mTypeDropDownMenu.options = GlobalHelper.EnumToDropDown<RobotUpgradeType>();
            DisplayTypeDropDownMenu();
        }

        private void DisplayTypeDropDownMenu()
        {
            var options = GetSubTypeOptions();
            mSubTypeDropDownMenu.value = 0;
            if (options == null)
            {
                mSubTypeDropDownMenu.gameObject.SetActive(false);
                return;
            }
            mSubTypeDropDownMenu.gameObject.SetActive(true);
            mSubTypeDropDownMenu.options = options;
        }

        private List<TMP_Dropdown.OptionData> GetSubTypeOptions ()
        {
            RobotUpgradeType type = (RobotUpgradeType)mTypeDropDownMenu.value;
            switch (type)
            {
                case RobotUpgradeType.Tool:
                    return GlobalHelper.EnumToDropDown<RobotToolType>();
                default:
                    return null;
            }
            
        }
        
        private void CreateClick()
        {
            string upgradeName = mInputField.text;
            if (string.IsNullOrEmpty(upgradeName)) return;
            
            RobotUpgradeType type = (RobotUpgradeType)mTypeDropDownMenu.value;
            int subType = (int)mSubTypeDropDownMenu.value;
            SerializedRobotUpgradeNodeNetwork nodeNetwork = new SerializedRobotUpgradeNodeNetwork(type,subType,new List<RobotUpgradeNodeData>());
            
            string folderPath = DevToolUtils.GetDevToolPath(DevTool.Upgrade);
            string path = Path.Combine(folderPath,upgradeName) + ".json";
            string data = JsonConvert.SerializeObject(nodeNetwork);
            File.WriteAllText(path, data);
            upgradeSelector.DisplayList();
            GameObject.Destroy(gameObject);
        }
    }
}
