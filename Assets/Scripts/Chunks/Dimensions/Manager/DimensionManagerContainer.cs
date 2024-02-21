using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DimensionModule {
    public class DimensionManagerContainer
    {
        private static DimensionManagerContainer instance;
        private DimensionManager dimensionManager;
        private DimensionManagerContainer() {
            dimensionManager = GameObject.Find("DimensionManager").GetComponent<DimensionManager>();
        }
        public static DimensionManagerContainer getInstance() {
            if (instance == null) {
                instance = new DimensionManagerContainer();
            }
            return instance;
        }
        public DimensionManager getManager() {
            return dimensionManager;
        }
    }
}

