using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Misc.RandomFrequency;
namespace WorldModule.Caves {
    public enum TilePlacementRestriction {
        None,
        Horizontal,
        Vertical,
        Rectangle,
        Circle
    }
    [System.Serializable]
    public class TileDistributionFrequency : IFrequencyListElement  {
        public TileItem tileItem;
        public uint frequency = 1;

        public int getFrequency()
        {
            return (int) frequency;
        }
    }
}