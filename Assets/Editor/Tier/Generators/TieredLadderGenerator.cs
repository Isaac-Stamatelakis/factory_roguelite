using System.Collections;
using System.Collections.Generic;
using System.IO;
using Item.GameStage;
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
            TileItem tileItem = (TileItem)tileEntityItemGenerationData.ItemGenerationData.ItemObject;
            tileItem.tileOptions.TransmutableColorOverride = tierItemInfoObject.PrimaryMaterial;
            tileItem.tile = defaultValues.Tiles.Ladder;
            tileItem.tileType = TileType.Object;

            Ladder ladder = (Ladder)tileEntityItemGenerationData.TileEntityObject;
            float bonusSpeed = 0;
            if (tierItemInfoObject.GameStageObject is TieredGameStage tieredGameStage)
            {
                int tier = (int)tieredGameStage.Tier;
                bonusSpeed = 2.5f * tier;
            }

            ladder.speed = 7.5f + bonusSpeed;

        }
    }
}