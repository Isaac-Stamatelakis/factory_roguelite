using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI {
    public interface ILongClickable {
        public void longClick();
    }
    public class LongClickHandler 
    {
        public float longClickDuration = 0.25f;
        private bool isPointerDown = false;
        private float pointerDownTimer = 0f;
        private ILongClickable longClickable;
        public LongClickHandler (ILongClickable longClickable) {
            this.longClickable = longClickable;
        }

        public void click() {
            isPointerDown = true;
            pointerDownTimer = 0f;
        }

        public void checkHoldStatus() {
            if (!isPointerDown) {
                return;
            }
            pointerDownTimer += Time.deltaTime;
        }

        public void release() {
            isPointerDown = false;
            if (pointerDownTimer >= longClickDuration) {
                longClickable.longClick();
            }
        }
    }
}

