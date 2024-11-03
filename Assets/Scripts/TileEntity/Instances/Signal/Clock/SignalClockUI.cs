using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UI;

namespace TileEntityModule.Instances.Signal {
    public class SignalClockUI : MonoBehaviour, ITileEntityUI<SignalClockInstance>, IAmountIteratorListener
    {
        [SerializeField] private TMP_InputField delayField;
        [SerializeField] private Toggle toggle;
        [SerializeField] private AmountIteratorUI amountIteratorUI;
        private SignalClockInstance instance;
        public void display(SignalClockInstance tileEntityInstance)
        {
            this.instance = tileEntityInstance;
            delayField.text = tileEntityInstance.ClockData.Time.ToString();
            delayField.onValueChanged.AddListener((string value) => {
                ValidateInput(value, tileEntityInstance);
            });
            toggle.isOn = tileEntityInstance.ClockData.Active;
            toggle.onValueChanged.AddListener((bool val) => {
                tileEntityInstance.ClockData.Active = val;
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
            if (value > instance.TileEntity.MaxTime) {
                delayField.text = instance.TileEntity.MaxTime.ToString();
            } else if (value < instance.TileEntity.MinTime) {
                delayField.text = instance.TileEntity.MinTime.ToString();
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
