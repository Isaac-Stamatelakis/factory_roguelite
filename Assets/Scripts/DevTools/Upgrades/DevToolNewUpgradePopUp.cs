using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Player.Tool;
using Robot.Upgrades;
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
        internal void Initialize(DevToolUpgradeSelector upgradeSelector)
        {
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
            RobotUpgradeType type = (RobotUpgradeType)mTypeDropDownMenu.value;
            int subType = (int)mSubTypeDropDownMenu.value;
            RobotUpgradeNodeNetwork nodeNetwork = new RobotUpgradeNodeNetwork(type,subType,new List<RobotUpgradeNode>());
            string upgradeName = mInputField.text;
            string folderPath = DevToolUtils.GetDevToolPath(DevTool.Upgrade);
            string path = Path.Combine(folderPath,upgradeName) + ".bin";
            string data = JsonConvert.SerializeObject(nodeNetwork);
            byte[] compressed = WorldLoadUtils.CompressString(data);
            File.WriteAllBytes(path, compressed);
            GameObject.Destroy(gameObject);
        }
    }
}
