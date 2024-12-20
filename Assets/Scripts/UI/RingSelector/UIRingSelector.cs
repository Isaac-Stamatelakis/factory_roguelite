using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.RingSelector
{
    public delegate void RingSelectorCallback(); 
    public struct RingSelectorComponent
    {
        public Sprite sprite;
        public string name;
        public RingSelectorCallback callback;

        public RingSelectorComponent(Sprite sprite, string name, RingSelectorCallback callback)
        {
            this.sprite = sprite;
            this.name = name;
            this.callback = callback;
        }
    }
    public class UIRingSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image selectedImage;
        [SerializeField] private TextMeshProUGUI title;
        private List<RingSelectorComponent> currentComponents;
        [SerializeField] private Transform selectableRingContainer;
        [SerializeField] private UIRingSelectorElement selectorPrefab;
        [SerializeField] private Color unselectedColor;
        private Image panel;
        public Color SelectedColor => panel.color;
        public Color UnselectedColor => unselectedColor;
        public int DisplayCount => currentComponents.Count;
        private UIRingSelectorElement[] activeComponents;
        private Image image;
        private Camera mainCamera;
        private bool inCenter;
        private int lastSelectedIndex = -1;

        public void Start()
        {
            panel = GetComponent<Image>();
            mainCamera = Camera.main;
            panel.alphaHitTestMinimumThreshold = 0.1f;
            // Testing
            List<RingSelectorComponent> components = new List<RingSelectorComponent>
            {
                new RingSelectorComponent(null, "test1", () => { selectIndex(0); }),
                new RingSelectorComponent(null, "test1", () => { selectIndex(1); }),
                new RingSelectorComponent(null, "test1", () => { selectIndex(1); }),
                new RingSelectorComponent(null, "test1", () => { selectIndex(1); }),
                new RingSelectorComponent(null, "test1", () => { selectIndex(1); }),
            };
            Display(components);
        }

        public void selectIndex(int index)
        {
            Debug.Log($"Selected Index: {index}");
        }

        public void Display(List<RingSelectorComponent> components)
        {
            this.currentComponents = components;
            GlobalHelper.deleteAllChildren(selectableRingContainer);
            activeComponents = new UIRingSelectorElement[currentComponents.Count];
            for (int i = 0; i < components.Count; i++)
            {
                RingSelectorComponent component = components[i];
                UIRingSelectorElement element = Instantiate(selectorPrefab, selectableRingContainer);
                element.Display(component, this, i);
                activeComponents[i] = element;
            }
        }

        public void Update()
        {
            if (inCenter)
            {
                return;
            }
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Vector2 dif = (Vector2)Input.mousePosition - screenCenter;
            float angle = Mathf.Atan2(dif.y, dif.x) * Mathf.Rad2Deg+180;
            float startAngle = (90 - 360f / currentComponents.Count);
            if (startAngle < 0)
            {
                startAngle += 360;
            }
            float adjustedAngle = angle - startAngle;
            if (adjustedAngle < 0)
            {
                adjustedAngle += 360;
            }
            int index = (int)(adjustedAngle / (360f / currentComponents.Count));
            if (lastSelectedIndex == index)
            {
                return;
            }

            if (lastSelectedIndex >= 0)
            {
                activeComponents[lastSelectedIndex].Panel.color = UnselectedColor;
            }
            lastSelectedIndex = index;
            activeComponents[index].Panel.color = SelectedColor;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (lastSelectedIndex >= 0)
            {
                activeComponents[lastSelectedIndex].Panel.color = UnselectedColor;
            }
            lastSelectedIndex = -1;
            inCenter = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            inCenter = false;
        }
    }

}
