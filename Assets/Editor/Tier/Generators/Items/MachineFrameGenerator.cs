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
    public class MachineFrameGenerator : TierItemGenerator
    {
        public MachineFrameGenerator(TierItemInfoObject tierItemInfoObject, TierItemGeneratorDefaults defaultValues, string generationPath) : base(tierItemInfoObject, defaultValues, generationPath)
        {
        }

        public override void Generate()
        {
            var itemGenerationData = GenerateDefaultItemData(TierGeneratedItemType.MachineFrame,ItemType.TileItem,useTierName:true);
            TileItem tileItem = (TileItem)itemGenerationData.ItemObject;
            tileItem.tileOptions.TransmutableColorOverride = tierItemInfoObject.PrimaryMaterial;
            tileItem.tile = defaultValues.Tiles.MachineFrame;
            tileItem.tileType = TileType.Object;

            RandomEditorItemSlot recipeOutput = itemGenerationData.ToRandomEditorSlot(1);
            
            EditorItemSlot plateInput = StateToItem(TransmutableItemState.Plate, 8);
            List<EditorItemSlot> inputs = new List<EditorItemSlot> { plateInput };
            AssignBasicItemRecipes(recipeOutput,inputs,50,itemGenerationData);
        }
    }
}