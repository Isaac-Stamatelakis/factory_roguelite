using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeModule.Processors {
    public abstract class RecipeProcessorUI : MonoBehaviour
    {
        /// <summary>
        /// Hides certain elements from the UI which are not needed in view only mode
        /// </summary>
        public abstract void setViewMode(); 
    }
}

