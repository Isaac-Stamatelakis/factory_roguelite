using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Indicators
{
    public class ConduitViewIndicatorUI : MonoBehaviour
    {
        [SerializeField] private Image mMatrixImage;
        [SerializeField] private Image mEnergyImage;
        [SerializeField] private Image mSignalImage;
        [SerializeField] private Image mItemImage;
        [SerializeField] private Image mFluidImage;

        public void Display(List<ConduitType> conduitTypes)
        {
            Dictionary<ConduitType, Image> conduitImages = new Dictionary<ConduitType, Image>
            {
                [ConduitType.Matrix] = mMatrixImage,
                [ConduitType.Energy] = mEnergyImage,
                [ConduitType.Signal] = mSignalImage,
                [ConduitType.Item] = mItemImage,
                [ConduitType.Fluid] = mFluidImage
            };
            foreach (ConduitType conduitType in conduitTypes)
            {
                conduitImages[conduitType].enabled = true;
            }
            
            Image centerImage = conduitImages[ConduitType.Signal];
            if (conduitTypes.Count == 0)
            {
                centerImage.enabled = true;
                centerImage.color = Color.gray;
            }
            else
            {
                centerImage.color = Color.white;
            }
        }
    }
}
