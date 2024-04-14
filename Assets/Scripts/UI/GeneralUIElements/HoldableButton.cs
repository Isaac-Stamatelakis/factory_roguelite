using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI {
    public interface IHoldButtonListener {
        public void callbackDown(Transform caller);
        public void callBackUp(Transform caller);
    }
    public class HoldableButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        
        private IHoldButtonListener listener;
        public void init(IHoldButtonListener listener) {
            this.listener = listener;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            listener.callbackDown(transform);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            listener.callBackUp(transform);
        }
    }
}

