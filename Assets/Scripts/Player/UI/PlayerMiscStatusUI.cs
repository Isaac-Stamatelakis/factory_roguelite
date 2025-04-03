using System;
using Dimensions;
using UI.Indicators;
using UnityEngine;

namespace Player.UI
{
    public class PlayerMiscStatusUI : MonoBehaviour
    {
        public CaveIndicatorUI CaveIndicatorUI;

        public void Start()
        {
            CaveIndicatorUI = GetComponentInChildren<CaveIndicatorUI>();
        }

        public void Initialize()
        {
            CaveIndicatorUI.Display((Dimension)DimensionManager.Instance.GetPlayerDimension());
        }
    }
}
