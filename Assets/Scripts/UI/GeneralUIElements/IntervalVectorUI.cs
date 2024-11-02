using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

namespace UI {
    public delegate void ReloadCallBack();
    public class IntervalVectorUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField xLower;
        [SerializeField] private TMP_InputField xUpper;
        [SerializeField] private TMP_InputField yLower;
        [SerializeField] private TMP_InputField yUpper;
        [SerializeField] private Button backButton;

        public void display(IntervalVector intervalVector, ReloadCallBack reloadCallBack) {
            xLower.contentType = TMP_InputField.ContentType.IntegerNumber;
            xUpper.contentType = TMP_InputField.ContentType.IntegerNumber;
            yLower.contentType = TMP_InputField.ContentType.IntegerNumber;
            yUpper.contentType = TMP_InputField.ContentType.IntegerNumber;

            xLower.text = intervalVector.X.LowerBound.ToString();
            xUpper.text = intervalVector.X.UpperBound.ToString();
            yLower.text = intervalVector.Y.LowerBound.ToString();
            yUpper.text = intervalVector.Y.UpperBound.ToString();

            // Add listener to xLower input field
            xLower.onValueChanged.AddListener((string value) => 
            {
                int? intValue = validate(value);
                if (intValue != null) {
                    intervalVector.X.LowerBound = (int)intValue;
                }
                
            });

            // Add listener to xUpper input field
            xUpper.onValueChanged.AddListener((string value) => 
            {
                int? intValue = validate(value);
                if (intValue != null) {
                    intervalVector.X.UpperBound = (int)intValue;
                }
            });

            // Add listener to yLower input field
            yLower.onValueChanged.AddListener((string value) => 
            {
                int? intValue = validate(value);
                if (intValue != null) {
                    intervalVector.Y.LowerBound = (int)intValue;
                }
            });

            // Add listener to yUpper input field
            yUpper.onValueChanged.AddListener((string value) => 
            {
                int? intValue = validate(value);
                if (intValue != null) {
                    intervalVector.Y.UpperBound = (int)intValue;
                }
            });

            backButton.onClick.AddListener(() => {
                reloadCallBack();
                GameObject.Destroy(gameObject);
            });
        }

        private int? validate(string value) {
            try {
                return Convert.ToInt32(value);
            } catch (FormatException) {
                return null;
            }
        }
    }
}

