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
    public class TileDistribution {
        private float Cover {get => 1/((float)density);}
        public float Frequency {get =>Cover/((minimumSize+maximumSize)/2f);}
        public uint density;
        public bool writeAll;
        public TilePlacementMode placementMode;
        [Range(0,4096)]
        public int minimumSize = 0;
        [Range(0,4096)]
        public int maximumSize = 4096;

        public List<TileDistributionFrequency> tiles;
        public TilePlacementRestriction restriction;
        [HideInInspector] public int minHeight = 0;
        [HideInInspector] public int maxHeight = 32767;
        [HideInInspector] public int minWidth = 0;
        [HideInInspector] public int maxWidth = 32767;
        [HideInInspector] public int minRadius = 0;
        [HideInInspector] public int maxRadius = 32767;
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