using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecipeModule;

namespace TileEntityModule.Instances.WorkBenchs {
    [CreateAssetMenu(fileName ="E~New WorkBench",menuName="Tile Entity/Workbench")]
    public class WorkBench : TileEntity, IClickableTileEntity, IProcessor
    {
        [SerializeField] public WorkBenchRecipeProcessor workBenchRecipeProcessor;
        [SerializeField] public GameObject uiPrefab;

        public HashSet<RecipeProcessor> getProcessors()
        {
            return new HashSet<RecipeProcessor>{workBenchRecipeProcessor};
        }

        public void onClick()
        {
            if (uiPrefab == null) {
                Debug.LogError(name + " uiPrefab is null");
                return;
            }
            GameObject instantiated = GameObject.Instantiate(uiPrefab);
            
        }
    }
}

