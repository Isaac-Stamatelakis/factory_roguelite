using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Catalogue.InfoViewer.Indicator
{
    public class CatalogueElementIndicatorNode : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] Image image;
        private int index;
        private Action<int> callback;

        public void Display(ICatalogueElement catalogueElement, int elementIndex, Action<int> parentCallback)
        {
            image.sprite = catalogueElement.GetSprite();
            if (catalogueElement is IColorableCatalogueElement colorableCatalogueElement)
            {
                image.color = colorableCatalogueElement.GetColor();
            }
            else
            {
                image.color = Color.white;
            }
            index = elementIndex;
            callback = parentCallback;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            callback.Invoke(index);
        }
    }
}
