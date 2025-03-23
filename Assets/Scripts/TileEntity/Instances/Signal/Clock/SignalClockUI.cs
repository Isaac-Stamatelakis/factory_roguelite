using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UI;

namespace TileEntity.Instances.Signal {
    public class SignalClockUI : MonoBehaviour, ITileEntityUI, IAmountIteratorListener
    {
        [SerializeField] private TMP_InputField delayField;
        [SerializeField] private Toggle toggle;
        [SerializeField] private AmountIteratorUI amountIteratorUI;
        private SignalClockInstance instance;
        public void DisplayTileEntityInstance(ITileEntityInstance tileEntityInstance)
        {
            if (tileEntityInstance is not SignalClockInstance signalClockInstance) return;
            this.instance = signalClockInstance;
            delayField.text = signalClockInstance.ClockData.Time.ToString();
            delayField.onValueChanged.AddListener((string value) => {
                ValidateInput(value, signalClockInstance);
            });
            toggle.isOn = signalClockInstance.ClockData.Active;
            toggle.onValueChanged.AddListener((bool val) => {
                signalClockInstance.ClockData.Active = val;
            });
            amountIteratorUI.setListener(this);

        }

        public void iterate(int amount)
        {
            int updateTime = instance.ClockData.Time + amount;
            delayField.text = updateTime.ToString();
            validRange(updateTime);
        }

        public void validRange(int value) {
            if (value > instance.TileEntityObject.MaxTime) {
                delayField.text = instance.TileEntityObject.MaxTime.ToString();
            } else if (value < instance.TileEntityObject.MinTime) {
                delayField.text = instance.TileEntityObject.MinTime.ToString();
            }
            instance.ClockData.Time = value;
        }

        private void ValidateInput(string input, SignalClockInstance instance)
        {
            string filteredInput = System.Text.RegularExpressions.Regex.Replace(input, "[^0-9]", "");

            if (string.IsNullOrEmpty(filteredInput))
            {
                delayField.text = string.Empty;
            }
            else
            {
                delayField.text = filteredInput;
                int value = Convert.ToInt32(filteredInput);
                validRange(value);
            }
        }
    }

}
