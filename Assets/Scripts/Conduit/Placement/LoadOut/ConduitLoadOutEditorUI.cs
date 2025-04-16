using System;
using System.Collections.Generic;
using Conduit.Port.UI;
using Conduits.Ports;
using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;


namespace Conduit.Placement.LoadOut
{
    public class IOConduitPortData
    {
        public ConduitPortData InputData;
        public ConduitPortData OutputData;
    }

    public enum LoadOutConduitType
    {
        ItemFluid = 0,
        Energy = 1,
        Signal = 2
    }
    public static class LoadOutConduitTypeExtension {
        public static ConduitType ToConduitType(this LoadOutConduitType type)
        {
            switch (type)
            {
                case LoadOutConduitType.ItemFluid: // Item and fluid share port data constructors so the return type doesn't matter here
                    return ConduitType.Item;
                case LoadOutConduitType.Energy:
                    return ConduitType.Energy;
                case LoadOutConduitType.Signal:
                    return ConduitType.Signal;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
    public class ConduitLoadOutEditorUI : MonoBehaviour
    {
        [SerializeField] private IOConduitPortUI mConduitPortUI;
        [SerializeField] private TextMeshProUGUI mTitleText;
        [SerializeField] private TMP_Dropdown mTypeDropDown;

        public void Display(Dictionary<LoadOutConduitType, IOConduitPortData> loadOutData, LoadOutConduitType loadOutType)
        {
            void SetDisplayElements(LoadOutConduitType loadOutConduitType)
            {
                mTypeDropDown.GetComponent<Image>().color = ConduitPortFactory.GetConduitPortColor(loadOutConduitType.ToConduitType());
                
                mTitleText.text = GetLoadOutText(loadOutConduitType) + " Port Placement";
            }

            string GetLoadOutText(LoadOutConduitType loadOutConduitType)
            {
                return loadOutConduitType switch
                {
                    LoadOutConduitType.ItemFluid => "Item & Fluid",
                    LoadOutConduitType.Energy => "Energy",
                    LoadOutConduitType.Signal => "Signal",
                    _ => throw new ArgumentOutOfRangeException(nameof(loadOutConduitType), loadOutConduitType, null)
                };
            }
            mConduitPortUI.Display(loadOutData[loadOutType]);
            SetDisplayElements(loadOutType);
            InitializeDropDown();
            
            return;
            void InitializeDropDown()
            {
                List<string> options = new List<string>();
                foreach (LoadOutConduitType loadOutConduitType in System.Enum.GetValues(typeof(LoadOutConduitType)))
                {
                    options.Add(GetLoadOutText(loadOutConduitType));
                }

                mTypeDropDown.options = GlobalHelper.StringListToDropDown(options);
                mTypeDropDown.value = (int)loadOutType;

                mTypeDropDown.onValueChanged.AddListener((value) =>
                {
                    LoadOutConduitType newLoadOut = (LoadOutConduitType)value;
                    mConduitPortUI.Display(loadOutData[newLoadOut]);
                    SetDisplayElements(newLoadOut);
                });
            }
        }
        
        public enum LoadOutType
        {
            ItemFluid,
            Energy,
            Signal
        }
    }
}
