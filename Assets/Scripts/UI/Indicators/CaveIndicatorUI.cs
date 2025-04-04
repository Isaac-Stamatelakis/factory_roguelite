using System;
using Dimensions;
using Player;
using Tiles;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Indicators
{
    public class CaveIndicatorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image mImage;
        [SerializeField] private Sprite arrowSprite;
        private Dimension currentDimension;
        private float distance;
        
        public void Display(Dimension currentDimension)
        {
            this.currentDimension = currentDimension;
            mImage.sprite = GetSprite();
            mImage.transform.rotation = Quaternion.identity;
            mImage.color = this.currentDimension == Dimension.Cave ? Color.magenta : Color.white;
            
        }
        

        private Sprite GetSprite()
        {
            switch (currentDimension)
            {
                case Dimension.OverWorld:
                    break;
                case Dimension.Cave:
                    return arrowSprite;
                case Dimension.CompactMachine:
                    break;
            }

            return null;
        }

        public void UpdateRotation(Vector2 direction, float distance)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            mImage.transform.rotation = Quaternion.Euler(0, 0, angle);
            this.distance = distance;
        }

        private string GetDimensionText()
        {
            switch (currentDimension)
            {
                case Dimension.OverWorld:
                    return "HUB";
                case Dimension.Cave:
                    return $"Cave: Distance From Portal {distance:F1}";
                case Dimension.CompactMachine:
                    return "Compact Machine";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            ToolTipController.Instance.ShowToolTip(transform.position, GetDimensionText(),reverse:true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }
        
    }
}
