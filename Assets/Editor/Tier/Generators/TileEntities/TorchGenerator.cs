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
    public class TorchGenerator : TierItemGenerator
    {
        public TorchGenerator(TierItemInfoObject tierItemInfoObject, TierItemGeneratorDefaults defaultValues, string generationPath) : base(tierItemInfoObject, defaultValues, generationPath)
        {
        }

        public override void Generate()
        {
            TileEntityItemGenerationData tileEntityItemGenerationData = GenerateDefaultTileEntityItemData<Torch>("Torch");
            TileItem tileItem = (TileItem)tileEntityItemGenerationData.ItemGenerationData.ItemObject;
            tileItem.tileOptions.TransmutableColorOverride = tierItemInfoObject.PrimaryMaterial;
            tileItem.tile = defaultValues.Tiles.TorchRod;
            tileItem.tileOptions.Overlay = defaultValues.Tiles.TorchSource;
            tileItem.tileType = TileType.Object;

            Torch torch = (Torch)tileEntityItemGenerationData.TileEntityObject;
            torch.intensity = 1;
            torch.radius = 7;
            torch.falloff = 0.7f;
            torch.color = Color.white;
            torch.positionInTile = Global.TILE_SIZE / 2 * Vector2.one;
            
            RandomEditorItemSlot recipeOutput = new RandomEditorItemSlot(tileEntityItemGenerationData.ItemGenerationData.ItemObject, 8, 1f);
            
            EditorItemSlot rodInput = StateToItem(TransmutableItemState.Rod, 3);
            
            // TODO LIGHT INPUT
            List<EditorItemSlot> inputs = new List<EditorItemSlot> { rodInput };
            AssignBasicItemRecipes(recipeOutput,inputs,50,tileEntityItemGenerationData.ItemGenerationData);
        }
    }
}