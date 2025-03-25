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
        public Color? color;
        public Sprite sprite;
        public string name;
        public RingSelectorCallback callback;

        public RingSelectorComponent(Color? color, Sprite sprite, string name, RingSelectorCallback callback)
        {
            this.color = color;
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
        [SerializeField] private Sprite unselectedSprite;
        private Image panel;
        public Color SelectedColor => panel.color;
        public Color UnselectedColor => unselectedColor;
        public int DisplayCount => currentComponents.Count;
        private UIRingSelectorElement[] activeComponents;
        private Image image;
        private Camera mainCamera;
        private bool inCenter;
        private int lastSelectedIndex = -1;
        private RingSelectorComponent defaultComponent;

        public void Start()
        {
            panel = GetComponent<Image>();
            mainCamera = Camera.main;
            panel.alphaHitTestMinimumThreshold = 0.1f;
        }

        public void selectIndex(int index)
        {
            Debug.Log($"Selected Index: {index}");
        }

        public void Display(List<RingSelectorComponent> components, RingSelectorComponent defaultComponent)
        {
            this.currentComponents = components;
            this.defaultComponent = defaultComponent;
            GlobalHelper.DeleteAllChildren(selectableRingContainer);
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
            if (Input.GetMouseButtonUp(0))
            {
                if (lastSelectedIndex == -1)
                {
                    defaultComponent.callback?.Invoke();
                }
                else
                {
                    currentComponents[lastSelectedIndex].callback?.Invoke();
                }
                CanvasController.Instance.PopStack();
                return;
            }
            if (inCenter)
            {
                panel.color = defaultComponent.color ?? UnselectedColor;
                title.text = defaultComponent.name;
                lastSelectedIndex = -1;
                selectedImage.enabled = true;
                selectedImage.sprite = unselectedSprite;
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
            
            
            lastSelectedIndex = index;
            var currentComponent = currentComponents[index];
            
            panel.color = currentComponent.color ?? SelectedColor;
            title.text = currentComponents[index].name;
            if (ReferenceEquals(currentComponent.sprite, null))
            {
                selectedImage.enabled = false;
            }
            else
            {
                selectedImage.enabled = true;
                selectedImage.sprite = currentComponent.sprite;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            lastSelectedIndex = -1;
            inCenter = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            inCenter = false;
        }
    }

}
