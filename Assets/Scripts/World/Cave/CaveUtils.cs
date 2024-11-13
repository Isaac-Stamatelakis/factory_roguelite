using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace WorldModule.Caves {
    
    public static class CaveUtils {
        public static IEnumerable LoadCaveElements(Cave cave) {
            var entityHandle = cave.entityDistributor.LoadAssetAsync<UnityEngine.Object>();
            var generationModelHandle = cave.generationModel.LoadAssetAsync<UnityEngine.Object>();
            
            yield return entityHandle;
            yield return generationModelHandle;
            
        }
    
        
    }
}

