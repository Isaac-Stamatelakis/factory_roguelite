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
    public class TieredChestGenerator : TierItemGenerator
    {
        public TieredChestGenerator(TierItemInfoObject tierItemInfoObject, TierItemGeneratorDefaults defaultValues, string generationPath) : base(tierItemInfoObject, defaultValues, generationPath)
        {
        }

        public override void Generate()
        {
            TileEntityItemGenerationData tileEntityItemGenerationData = GenerateDefaultTileEntityItemData<Chest>("Chest");
            TileItem tileItem = (TileItem)tileEntityItemGenerationData.ItemGenerationData.ItemObject;
            tileItem.tileOptions.TransmutableColorOverride = tierItemInfoObject.PrimaryMaterial;
            tileItem.tile = defaultValues.Tiles.Chest;
            tileItem.tileType = TileType.Object;

            Chest chest = (Chest)tileEntityItemGenerationData.TileEntityObject;
            uint bonusRows = 0;
            uint bonusColumns = 0;
            const int MAX_BONUS_ROWS = 4;
            const int MAX_BONUS_COLUMNS = 10;
            if (tierItemInfoObject.GameStageObject is TieredGameStage tieredGameStage)
            {
                uint tier = (uint)tieredGameStage.Tier;
                bonusRows = 2*tier;
                if (bonusRows > MAX_BONUS_ROWS)
                {
                    uint leftOver = bonusRows - MAX_BONUS_ROWS;
                    bonusRows = MAX_BONUS_ROWS;
                    bonusColumns = leftOver;
                }
                if (bonusColumns > MAX_BONUS_COLUMNS) bonusColumns = MAX_BONUS_COLUMNS;
            }

            chest.Rows = 4 + bonusRows;
            chest.Columns = 10 + bonusColumns;
            chest.AssetReference = defaultValues.UIReferences.ChestUI;
            chest.ConduitLayout = defaultValues.ConduitPortLayouts.ItemInOut;
            
            RandomEditorItemSlot recipeOutput = new RandomEditorItemSlot(tileEntityItemGenerationData.ItemGenerationData.ItemObject, 16, 1f);
            
            EditorItemSlot plateInput = StateToItem(TransmutableItemState.Plate, 4);
            EditorItemSlot screwInput = StateToItem(TransmutableItemState.Screw, 8);
            List<EditorItemSlot> inputs = new List<EditorItemSlot> { plateInput, screwInput };
            AssignBasicItemRecipes(recipeOutput,inputs,50,tileEntityItemGenerationData.ItemGenerationData);
        }
    }
}