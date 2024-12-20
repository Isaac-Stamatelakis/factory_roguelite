using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.RingSelector
{
    public class UIRingSelectorElement : MonoBehaviour
    {
        [SerializeField] private Image image;
        private UIRingSelector selector;
        private RingSelectorComponent component;
        private Image panel;
        public Image Panel { get { return panel; } }
        public void Display(RingSelectorComponent displayComponent, UIRingSelector ringSelector, int index)
        {
            this.selector = ringSelector;
            this.component = displayComponent;
            panel = GetComponent<Image>();
            panel.color = ringSelector.UnselectedColor;
            panel.fillAmount = 1f / ringSelector.DisplayCount-0.001f;
            float angle = index * 360f / ringSelector.DisplayCount;
            transform.localEulerAngles += new Vector3(0, 0, angle);
            image.transform.localEulerAngles -= new Vector3(0, 0, angle); // Image is child of component undo rotation
            float angleMidPoint =90-180f/ringSelector.DisplayCount;
            image.transform.localPosition = -350*new Vector3(MathF.Cos(angleMidPoint*Mathf.Deg2Rad), MathF.Sin(angleMidPoint*Mathf.Deg2Rad), 0);
        }
    }
}

