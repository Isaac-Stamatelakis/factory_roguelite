using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

namespace UI {
    public class AmountIteratorUI : MonoBehaviour, IHoldButtonListener
    {
        private enum Mode {
            Up,
            Down
        }
        private Mode? mode;
        [SerializeField] private Button up;
        [SerializeField] private Button down;
        [SerializeField] private HoldableButton upHold;
        [SerializeField] private HoldableButton downHold;
        private float timeHeld = 0f;
        private int updateCount = 0;
        
        private TMP_InputField inputField;
        
        public void init(Transform parent, TMP_InputField inputField) {
            this.inputField = inputField;
            transform.SetParent(parent,false);
            up.onClick.AddListener(() => {
                iterateAmount(1);
            });
            down.onClick.AddListener(() => {
                iterateAmount(-1);
            });
            upHold.init(this);
            downHold.init(this);
        }

        public static AmountIteratorUI newInstance() {
            return GlobalHelper.instantiateFromResourcePath("UI/General/AmountIterator").GetComponent<AmountIteratorUI>();
        }
        public void callbackDown(Transform caller)
        {
            timeHeld = 0f;
            updateCount = 0;
            if (caller.Equals(upHold.transform)) {
                mode = Mode.Up;
            } else if (caller.Equals(downHold.transform)) {
                mode = Mode.Down;
            }
            
        }
        public void callBackUp(Transform caller)
        {
            mode = null;
        }

        public void FixedUpdate() {
            if (mode == null) {
                return;
            }
            updateCount ++;
            timeHeld += Time.deltaTime;
            if (updateCount < 5) {
                return;
            }
            updateCount = 0;
            if (timeHeld < 0.25f) {
                return;
            }
            float adjustedTime = timeHeld+0.75f;
            int toChange = Mathf.FloorToInt((Mathf.Pow(adjustedTime,3)));
            switch (mode) {
                case Mode.Up:
                    iterateAmount(toChange);
                    break;    
                case Mode.Down:
                    iterateAmount(-toChange);
                    break;
            }
        }

        private void iterateAmount(int iterator) {
            int amount = Convert.ToInt32(inputField.text);
            amount += iterator;
            amount = Mathf.Max(1,amount);
            inputField.text = amount.ToString();
        }

        
    }
}

