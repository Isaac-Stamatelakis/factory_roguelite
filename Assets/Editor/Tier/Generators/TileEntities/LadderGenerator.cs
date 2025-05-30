using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Item.GameStage;
using Item.Transmutation;
using Items;
using Items.Transmutable;
using Recipe.Objects;
using Tier.Generators.Defaults;
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

        protected override ITierGeneratedItemData GenerateItemData()
        {
            TileEntityItemGenerationData tileEntityItemGenerationData = GenerateDefaultTileEntityItemData<Ladder>(TierGeneratedItemType.Ladder);
            TileItem tileItem = (TileItem)tileEntityItemGenerationData.ItemGenerationData.ItemObject;
            tileItem.tileOptions.TransmutableColorOverride = tierItemInfoObject.PrimaryMaterial;
            tileItem.tile = defaultValues.Tiles.Ladder;
            tileItem.tileType = TileType.Object;

            Ladder ladder = (Ladder)tileEntityItemGenerationData.TileEntityObject;
            float bonusSpeed = 0; 
            if (tierItemInfoObject.GameStageObject is TieredGameStage tieredGameStage)
            {
                int tier = (int)tieredGameStage.Tier;
                bonusSpeed = 2.5f * tier + 0.25f * tier * tier;;
            }

            ladder.speed = 7.5f + bonusSpeed;
            RandomEditorItemSlot recipeOutput = new RandomEditorItemSlot(tileEntityItemGenerationData.ItemGenerationData.ItemObject, 16, 1f);
            
            EditorItemSlot rodInput = StateToItem(TransmutableItemState.Rod, 6);
            EditorItemSlot screwInput = StateToItem(TransmutableItemState.Screw, 9);
            List<EditorItemSlot> inputs = new List<EditorItemSlot> { rodInput, screwInput };
            AssignBasicItemRecipes(recipeOutput,inputs,50,tileEntityItemGenerationData.ItemGenerationData);
            return tileEntityItemGenerationData;
        }
    }
}