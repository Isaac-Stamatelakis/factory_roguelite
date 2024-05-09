using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dimensions {
    public class DimensionManagerContainer
    {
        private static DimensionManagerContainer instance;
        private static DimensionManager dimensionManager;
        private DimensionManagerContainer() {
            DimensionManager[] managers = GameObject.Find("DimensionManager").GetComponents<DimensionManager>();
            if (managers.Length == 0) {
                Debug.LogError("Dimension Manager Container could not find manager");
                return;
            } else if (managers.Length > 1) {
                Debug.LogError("Dimension Manager object has multiple managers");
            }
            dimensionManager = managers[0];
        }
        public static DimensionManagerContainer getInstance() {
            if (instance == null) {
                instance = new DimensionManagerContainer();
            }
            return instance;
        }
        public static DimensionManager getManager() {
            if (instance == null) {
                instance = new DimensionManagerContainer();
            }
            return dimensionManager;
        }
    }
}

