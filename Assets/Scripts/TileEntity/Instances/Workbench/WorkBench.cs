using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecipeModule;
using Chunks;

namespace TileEntityModule.Instances.WorkBenchs {
    [CreateAssetMenu(fileName ="E~New WorkBench",menuName="Tile Entity/Workbench")]
    public class WorkBench : TileEntity
    {
        [SerializeField] public WorkBenchRecipeProcessor WorkBenchRecipeProcessor;
        [SerializeField] public TileEntityUIManager UIManager;

        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new WorkBenchInstance(this,tilePosition,tileItem,chunk);
        }
    }
}

