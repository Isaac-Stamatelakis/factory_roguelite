using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecipeModule;
using Chunks;
using Recipe.Processor;
using TileEntity.Instances.WorkBench;

namespace TileEntity.Instances.WorkBenchs {
    public class WorkBenchInstance : TileEntityInstance<WorkBench>, IRightClickableTileEntity
    {
        public WorkBenchInstance(WorkBench tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public void onRightClick()
        {
            tileEntityObject.UIAssetManager.display<WorkBenchInstance,WorkBenchUI>(this);
        }
    }
}

