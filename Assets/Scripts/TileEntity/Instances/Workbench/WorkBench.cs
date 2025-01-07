using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecipeModule;
using Chunks;
using Recipe.Processor;
using UI;
using UnityEditor;

namespace TileEntity.Instances.WorkBenchs {
    [CreateAssetMenu(fileName ="New WorkBench",menuName="Tile Entity/Workbench")]
    public class WorkBench : TileEntityObject, IProcessorTileEntity, IManagedUITileEntity
    {
        public RecipeProcessor WorkBenchRecipeProcessor;
        public TileEntityUIManager UIAssetManager;
        public bool HasInventory;
        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new WorkBenchInstance(this,tilePosition,tileItem,chunk);
        }

        public RecipeProcessor GetRecipeProcessor()
        {
            return WorkBenchRecipeProcessor;
        }

        public TileEntityUIManager getUIManager()
        {
            return UIAssetManager;
        }
    }
    
    public interface IProcessorTileEntity {
        public RecipeProcessor GetRecipeProcessor();
    }
}

