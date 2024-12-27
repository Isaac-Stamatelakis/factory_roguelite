using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace RecipeModule.Viewer {

    public enum RecipeProcessorPosition {
        Center,
        Left,
        Right
    }

    public class RecipeProcessorIndicator : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] public Image image;
        [SerializeField] public RecipeProcessorPosition position;
        [SerializeField] public int index;
        private RecipeProcessorIndicatorController controller;

        public void OnPointerClick(PointerEventData eventData)
        {
            int offset = 0;
            if (position == RecipeProcessorPosition.Left) {
                offset = -(index+1);
            } else if (position == RecipeProcessorPosition.Right) {
                offset = index+1;
            }

            if (eventData.button == PointerEventData.InputButton.Left) {
                controller.RecipeViewer.MoveByAmount(offset);
                
            } else if (eventData.button == PointerEventData.InputButton.Right) {
                controller.RecipeViewer.DisplayUsesOfProcessor(offset);
            }
        }

        public void setImage(Image image) {
            this.image = image;
        }

        public void init(RecipeProcessorIndicatorController controller) {
            this.controller = controller;
        }
    }
}
