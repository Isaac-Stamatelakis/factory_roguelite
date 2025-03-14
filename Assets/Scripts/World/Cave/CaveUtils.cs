using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;
using System.Linq;
using Unity.VisualScripting;

namespace WorldModule.Caves {
    
    public static class CaveUtils {
        public static IEnumerable LoadCaveElements(CaveObject caveObject) {
            var entityHandle = caveObject.entityDistributor.LoadAssetAsync<UnityEngine.Object>();
            var generationModelHandle = caveObject.generationModel.LoadAssetAsync<UnityEngine.Object>();
            
            yield return entityHandle;
            yield return generationModelHandle;
            
        }

        public static string IdFromName(string name)
        {
            return name.ToLower().Replace(" ", "_");
        }

        public static string NameFromId(string id)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(id.Replace("_"," "));
        }
    
        
    }
}

