using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Misc {
    public class ResourceGarbageCollector : MonoBehaviour
    {
        private int counter;
        public void FixedUpdate() {
            counter ++;
            if (counter > 7500) { // 5 minutes
                Resources.UnloadUnusedAssets();
            }
        }
    }
}

