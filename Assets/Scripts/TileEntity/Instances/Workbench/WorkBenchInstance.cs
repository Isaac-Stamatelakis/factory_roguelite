using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecipeModule;
using Chunks;
using Recipe.Processor;

namespace TileEntity.Instances.WorkBenchs {
    public class WorkBenchInstance : TileEntityInstance<WorkBench>, IRightClickableTileEntity, IProcessorTileEntity
    {
        public WorkBenchInstance(WorkBench tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public RecipeProcessor GetRecipeProcessor()
        {
            return TileEntityObject.WorkBenchRecipeProcessor;
        }

        public void onRightClick()
        {
            GameObject uiPrefab = TileEntityObject.WorkBenchRecipeProcessor.UIPrefab;
            if (uiPrefab == null) {
                Debug.LogError(TileEntityObject.name + " uiPrefab is null");
                return;
            }
            GameObject instantiated = GameObject.Instantiate(uiPrefab);
            
        }
    }
}

