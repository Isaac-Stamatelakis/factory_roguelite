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
    public class RobotArmGenerator : TierItemGenerator
    {
        public RobotArmGenerator(TierItemInfoObject tierItemInfoObject, TierItemGeneratorDefaults defaultValues, string generationPath) : base(tierItemInfoObject, defaultValues, generationPath)
        {
        }

        public override void Generate()
        {
            var itemGenerationData = GenerateDefaultItemData(TierGeneratedItemType.RobotArm,ItemType.Crafting,useTierName:true);
            ItemObject itemObject = itemGenerationData.ItemObject;
            RobotArmSprites robotArmSprites = defaultValues.ItemSprites.RobotArmSprites;
            itemObject.SpriteOverlays = new SpriteOverlay[2];
            itemObject.SpriteOverlays[0] = new SpriteOverlay
            {
                Color = tierItemInfoObject.PrimaryMaterial.color,
                Sprite = robotArmSprites.MainSprite
            };
            itemObject.SpriteOverlays[1] = new SpriteOverlay
            {
                Color = tierItemInfoObject.SecondaryMaterial.color,
                Sprite = robotArmSprites.SecondarySprite
            };
            
            RandomEditorItemSlot recipeOutput = itemGenerationData.ToRandomEditorSlot(1);
            ItemObject motorItem = LookUpGeneratedItem(TierGeneratedItemType.Motor);
            EditorItemSlot motorInput = new EditorItemSlot(motorItem, 2);
            
            EditorItemSlot plateInput = StateToItem(TransmutableItemState.Plate, 8);
            List<EditorItemSlot> inputs = new List<EditorItemSlot> { motorInput, plateInput };
            AssignBasicItemRecipes(recipeOutput,inputs,50,itemGenerationData);
        }

        [System.Serializable]
        public class RobotArmSprites
        {
            public Sprite MainSprite;
            public Sprite SecondarySprite;
        }
    }
}