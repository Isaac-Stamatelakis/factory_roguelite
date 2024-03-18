using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI {
    public class MouseHoverDetection : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private static GameObject currentHoveredObject;
        public void OnPointerEnter(PointerEventData eventData)
        {
            currentHoveredObject = EventSystem.current.currentSelectedGameObject;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (currentHoveredObject == gameObject) {
                currentHoveredObject = null;
            }
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void FixedUpdate()
        {
            
        }
    }
}

