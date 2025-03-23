using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecipeModule;
using Chunks;
using Recipe.Processor;
using UI;
using UnityEditor;
using UnityEngine.AddressableAssets;

namespace TileEntity.Instances.WorkBenchs {
    [CreateAssetMenu(fileName ="New WorkBench",menuName="Tile Entity/Workbench")]
    public class WorkBench : TileEntityObject, IProcessorTileEntity, IUITileEntity
    {
        public RecipeProcessor WorkBenchRecipeProcessor;
        public AssetReference AssetReference;
        public bool HasInventory;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new WorkBenchInstance(this,tilePosition,tileItem,chunk);
        }

        public RecipeProcessor GetRecipeProcessor()
        {
            return WorkBenchRecipeProcessor;
        }

        public AssetReference GetUIAssetReference()
        {
            return AssetReference;
        }
    }
    
    public interface IProcessorTileEntity {
        public RecipeProcessor GetRecipeProcessor();
    }
}

