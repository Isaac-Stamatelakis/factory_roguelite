using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Misc.RandomFrequency {
    public interface IFrequencyListElement {
        public int getFrequency();
    }
    public static class RandomFrequencyListUtils {
        public static T getRandomFromList<T>(List<T> elements) where T : IFrequencyListElement {
            int totalWeight = 0;
            foreach (T element in elements) {
                totalWeight += element.getFrequency();
            }
            int ran = UnityEngine.Random.Range(0,totalWeight);  
            int cumulativeWeight = 0;
            foreach (T element in elements) {
                cumulativeWeight += element.getFrequency();
                if (ran < cumulativeWeight) {
                    return element;
                }
            }
            return default(T);
        }
    }
}
