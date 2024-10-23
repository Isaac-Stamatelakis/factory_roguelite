using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecipeModule;
using Chunks;

namespace TileEntityModule.Instances.WorkBenchs {
    public class WorkBenchInstance : TileEntityInstance<WorkBench>, IRightClickableTileEntity, IProcessorTileEntity
    {
        [SerializeField] public WorkBenchRecipeProcessor workBenchRecipeProcessor;
        [SerializeField] public GameObject uiPrefab;

        public WorkBenchInstance(WorkBench tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public RecipeProcessor getRecipeProcessor()
        {
            return workBenchRecipeProcessor;
        }

        public void onRightClick()
        {
            TileEntityUIManager uIManager = tileEntity.UIManager;
            GameObject uiPrefab = uIManager.getUIElement();
            if (uiPrefab == null) {
                Debug.LogError(tileEntity.name + " uiPrefab is null");
                return;
            }
            GameObject instantiated = GameObject.Instantiate(uiPrefab);
            
        }
    }
}

