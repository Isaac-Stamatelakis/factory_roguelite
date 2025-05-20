using System.Collections;
using System.Collections.Generic;
using System.IO;
using Item.GameStage;
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
    public class PlatformGenerator : TierItemGenerator
    {
        public PlatformGenerator(TierItemInfoObject tierItemInfoObject, TierItemGeneratorDefaults defaultValues, string generationPath) : base(tierItemInfoObject, defaultValues, generationPath)
        {
        }

        public override void Generate()
        {
            ItemGenerationData itemGenerationData = GenerateDefaultItemData("Platform",ItemType.TileItem);
            TileItem tileItem = (TileItem)itemGenerationData.ItemObject;
            tileItem.tileOptions.TransmutableColorOverride = tierItemInfoObject.PrimaryMaterial;
            tileItem.tile = defaultValues.Tiles.Platform;
            tileItem.tileType = TileType.Platform;
            
            RandomEditorItemSlot recipeOutput = new RandomEditorItemSlot(itemGenerationData.ItemObject, 16, 1f);
            
            EditorItemSlot plateInput = StateToItem(TransmutableItemState.Plate, 3);
            List<EditorItemSlot> inputs = new List<EditorItemSlot> { plateInput};
            AssignBasicItemRecipes(recipeOutput,inputs,50,itemGenerationData);
        }
    }
}