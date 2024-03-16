using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RecipeModule.Viewer {

    public enum RecipeProcessorPosition {
        Center,
        Left,
        Right
    }

    public class RecipeProcessorIndicator : MonoBehaviour
    {
        [SerializeField] public Image image;
        [SerializeField] public RecipeProcessorPosition position;
        [SerializeField] public int index;
        public void setImage(Image image) {
            this.image = image;
        }
    }
}
