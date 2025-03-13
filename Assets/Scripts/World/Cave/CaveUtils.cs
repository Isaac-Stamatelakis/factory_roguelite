using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace WorldModule.Caves {
    
    public static class CaveUtils {
        public static IEnumerable LoadCaveElements(CaveObject caveObject) {
            var entityHandle = caveObject.entityDistributor.LoadAssetAsync<UnityEngine.Object>();
            var generationModelHandle = caveObject.generationModel.LoadAssetAsync<UnityEngine.Object>();
            
            yield return entityHandle;
            yield return generationModelHandle;
            
        }
    
        
    }
}

