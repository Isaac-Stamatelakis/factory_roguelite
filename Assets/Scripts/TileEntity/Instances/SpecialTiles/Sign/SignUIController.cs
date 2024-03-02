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

            line1In.onValueChanged.AddListener( (string value) => signData.line1 = value);
            line2In.onValueChanged.AddListener( (string value) => signData.line2 = value);
            line3In.onValueChanged.AddListener( (string value) => signData.line3 = value);
        }
        public void OnDestroy() {
            line1In.onValueChanged.RemoveAllListeners();
            line2In.onValueChanged.RemoveAllListeners();
            line3In.onValueChanged.RemoveAllListeners();
        }


    }
}

