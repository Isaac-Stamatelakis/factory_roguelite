using System;
using System.Collections.Generic;
using Conduit.View;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Indicators
{
    public class ConduitViewIndicatorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image mMatrixImage;
        [SerializeField] private Image mEnergyImage;
        [SerializeField] private Image mSignalImage;
        [SerializeField] private Image mItemImage;
        [SerializeField] private Image mFluidImage;

        public void Start()
        {
            mMatrixImage.enabled = false;
            mEnergyImage.enabled = false;
            mSignalImage.enabled = false;
            mItemImage.enabled = false;
            mFluidImage.enabled = false;
        }

        private ConduitViewOptions currentViewOptions;
        public void Display(ConduitViewOptions conduitViewOptions)
        {
            Dictionary<ConduitType, Image> conduitImages = new Dictionary<ConduitType, Image>
            {
                [ConduitType.Matrix] = mMatrixImage,
                [ConduitType.Energy] = mEnergyImage,
                [ConduitType.Signal] = mSignalImage,
                [ConduitType.Item] = mItemImage,
                [ConduitType.Fluid] = mFluidImage
            };
            
            Image centerImage = conduitImages[ConduitType.Signal];
            switch (conduitViewOptions.ConduitViewMode)
            {
                case ConduitViewMode.None:
                    centerImage.enabled = true;
                    centerImage.color = Color.gray;
                    return;
                case ConduitViewMode.Auto:
                    centerImage.enabled = true;
                    centerImage.color = Color.cyan;
                    return;
            }

            var conduitTypes = GetViewConduitTypes(conduitViewOptions);
            foreach (ConduitType conduitType in conduitTypes)
            {
                conduitImages[conduitType].enabled = true;
            }
            
            centerImage.color = Color.white;
        }

        private List<ConduitType> GetViewConduitTypes(ConduitViewOptions conduitViewOptions)
        {
            switch (conduitViewOptions.ConduitViewMode)
            {
                case ConduitViewMode.All:
                    return new List<ConduitType>
                    {
                        ConduitType.Matrix,
                        ConduitType.Energy,
                        ConduitType.Signal,
                        ConduitType.Item,
                        ConduitType.Fluid
                    };
                case ConduitViewMode.WhiteList:
                    ConduitType? whiteListType = conduitViewOptions.WhiteListType;
                    if (whiteListType == null) return null;
                    return new List<ConduitType>
                    {
                        whiteListType.Value
                    };
                default:
                    return null;
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            ToolTipController.Instance.ShowToolTip(transform.position, $"Conduit View Mode:  {currentViewOptions?.ConduitViewMode}");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolTipController.Instance.HideToolTip();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            
        }
    }
}
