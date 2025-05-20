using System.Collections;
using System.Collections.Generic;
using System.IO;
using Items;
using TileEntity;
using TileEntity.Instances;
using UnityEditor;
using UnityEngine;

namespace EditorScripts.Tier.Generators
{
    public class TieredLadderGenerator : TierItemGenerator
    {
        public TieredLadderGenerator(TierItemInfoObject tierItemInfoObject, TierItemGeneratorDefaults defaultValues, string generationPath) : base(tierItemInfoObject, defaultValues, generationPath)
        {
        }

        public override void Generate()
        {
            TileEntityItemGenerationData tileEntityItemGenerationData = GenerateDefaultTileEntityItemData<Ladder>("Ladder",RecipeGenerationMode.All);
        }
    }
}