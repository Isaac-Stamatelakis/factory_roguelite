using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Misc.RandomFrequency;

namespace WorldModule.Caves {
    public class Structure : ScriptableObject
    {
        public List<StructureVariant> variants = new List<StructureVariant>();
    }

    [System.Serializable]
    public class StructureVariant : IFrequencyListElement {
        [SerializeField] private string data;
        [SerializeField] private Vector2Int size;
        [SerializeField] private int frequency;

        public StructureVariant(string data, Vector2Int size, int frequency)
        {
            this.data = data;
            this.size = size;
            this.frequency = frequency;
        }

        public string Data { get => data; }
        public Vector2Int Size { get => size;  }
        public int Frequency { get => frequency; set => frequency = value;}

        public int getFrequency()
        {
            return frequency;
        }
    }

}
