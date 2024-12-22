using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TileEntityModule.Instances.Signs {
    public class SignUIController : MonoBehaviour
    {
        [SerializeField] public TMP_InputField line1In;
        [SerializeField] public TMP_InputField line2In;
        [SerializeField] public TMP_InputField line3In;
        private SignData signData;
        public void init(SignData signData) {
            this.signData = signData;
            line1In.text = signData.line1;
            line2In.text = signData.line2;
            line3In.text = signData.line3;
            
            AddInputFieldListener(line1In, value => signData.line1 = value);
            AddInputFieldListener(line2In, value => signData.line2 = value);
            AddInputFieldListener(line3In, value => signData.line3 = value);
        }

        private void AddInputFieldListener(TMP_InputField inputField, System.Action<string> onValueChanged)
        {
            inputField.onValueChanged.AddListener((string value) =>
            {
                if (Input.GetKey(KeyCode.Escape))
                {
                    return;
                }
                onValueChanged?.Invoke(value);
            });
        }
        public void OnDestroy() {
            line1In.onValueChanged.RemoveAllListeners();
            line2In.onValueChanged.RemoveAllListeners();
            line3In.onValueChanged.RemoveAllListeners();
        }


    }
}

