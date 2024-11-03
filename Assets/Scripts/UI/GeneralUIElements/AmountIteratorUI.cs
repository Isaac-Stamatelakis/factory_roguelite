using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

namespace UI {

    public interface IAmountIteratorListener {
        public void iterate(int amount);
    }
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
        private IAmountIteratorListener listener;
        public void setListener(IAmountIteratorListener listener) {
            this.listener = listener;
            up.onClick.AddListener(() => {
                listener.iterate(1);
            });
            down.onClick.AddListener(() => {
                listener.iterate(-1);
            });
            upHold.init(this);
            downHold.init(this);
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
            if (mode == Mode.Down) {
                toChange *= -1;
            }
            listener.iterate(toChange);
        }
    }
}

