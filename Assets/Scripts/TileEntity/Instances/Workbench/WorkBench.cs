using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecipeModule;
using Chunks;
using Recipe.Processor;
using UnityEditor;

namespace TileEntity.Instances.WorkBenchs {
    [CreateAssetMenu(fileName ="New WorkBench",menuName="Tile Entity/Workbench")]
    public class WorkBench : TileEntityObject, IProcessorTileEntity
    {
        public RecipeProcessor WorkBenchRecipeProcessor;

        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new WorkBenchInstance(this,tilePosition,tileItem,chunk);
        }

        public RecipeProcessor GetRecipeProcessor()
        {
            return WorkBenchRecipeProcessor;
        }
    }
    
    public interface IProcessorTileEntity {
        public RecipeProcessor GetRecipeProcessor();
    }
}

