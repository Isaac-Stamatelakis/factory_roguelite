using System;
using Player;
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
        private ICatalogueElement catalogueElement;
        private PlayerGameStageCollection gameStageCollection;

        public void Display(ICatalogueElement catalogueElement, int elementIndex, Action<int> parentCallback, PlayerGameStageCollection gameStageCollection)
        {
            this.gameStageCollection = gameStageCollection;
            this.catalogueElement = catalogueElement;
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
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    callback.Invoke(index);
                    break;
                case PointerEventData.InputButton.Right:
                    catalogueElement.DisplayAllElements(gameStageCollection);
                    break;
                default:
                    break;
            }
        }
    }
}
