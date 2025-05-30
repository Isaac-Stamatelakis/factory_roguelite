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
    public class TierCraftingItemGenerator : TierItemGenerator
    {
        private TierGeneratedCraftingItemType itemType;
        public TierCraftingItemGenerator(TierItemInfoObject tierItemInfoObject, TierItemGeneratorDefaults defaultValues, string generationPath, TierGeneratedCraftingItemType itemType) : base(tierItemInfoObject, defaultValues, generationPath)
        {
            this.itemType = itemType;
        }

        protected override ITierGeneratedItemData GenerateItemData()
        {
            TierCraftingItemGenerationData generationData = GetData();
            
            var itemGenerationData = GenerateDefaultItemData((TierGeneratedItemType)itemType,ItemType.Crafting,useTierName:generationData.UseTierName);
            ItemObject itemObject = itemGenerationData.ItemObject;
            int overlayCount = GetOverlayCount(generationData);
            itemObject.SpriteOverlays = new SpriteOverlay[overlayCount];
            
            int index = 0;
            if (generationData.PrimaryMaterialSprite) AddOverlaySprite( generationData.PrimaryMaterialSprite, tierItemInfoObject.PrimaryMaterial?.color);
            if (generationData.SecondaryMaterialSprite) AddOverlaySprite( generationData.SecondaryMaterialSprite, tierItemInfoObject.SecondaryMaterial?.color);
            if (generationData.StaticSprite) AddOverlaySprite(generationData.StaticSprite,Color.white);
            
            RandomEditorItemSlot recipeOutput = itemGenerationData.ToRandomEditorSlot(generationData.OutputAmount);
            TileEntity.Tier tier = tierItemInfoObject.GameStageObject is TieredGameStage tieredGameStage ? tieredGameStage.Tier : 0;
            List<EditorItemSlot> inputs = generationData.AdvancedInputCost == null || tier < TileEntity.Tier.Paramount
                ? generationData.BasicInputCost
                : generationData.AdvancedInputCost;
            
            AssignBasicItemRecipes(recipeOutput,inputs,generationData.TickCount,itemGenerationData);

            return itemGenerationData;
            void AddOverlaySprite(Sprite sprite, Color? color)
            {
                itemObject.SpriteOverlays[index] = new SpriteOverlay
                {
                    Color = color ?? Color.white,
                    Sprite = sprite
                };
                index++;
            }
        }
        
        private TierCraftingItemGenerationData GetData()
        {
            switch (itemType)
            {
                case TierGeneratedCraftingItemType.Motor:
                {
                    EditorItemSlot plateInput = StateToItem(TransmutableItemState.Plate, 8);
                    List<EditorItemSlot> inputs = new List<EditorItemSlot>
                    {
                        plateInput
                    };
                    return new TierCraftingItemGenerationData(defaultValues.ItemSprites.MotorSprite,null,null,1,50,true,inputs,null);
                }
                case TierGeneratedCraftingItemType.RobotArm:
                {
                    ItemObject motorItem = LookUpGeneratedItem(TierGeneratedItemType.Motor);
                    EditorItemSlot motorInput = new EditorItemSlot(motorItem, 2);
            
                    EditorItemSlot plateInput = StateToItem(TransmutableItemState.Plate, 8);
                    List<EditorItemSlot> inputs = new List<EditorItemSlot> { motorInput, plateInput };
                    return new TierCraftingItemGenerationData(defaultValues.ItemSprites.RobotArmSprites.PrimarySprite,defaultValues.ItemSprites.RobotArmSprites.SecondarySprite,null,1,50,true,inputs,null);
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null);
            }
        }

        private int GetOverlayCount(TierCraftingItemGenerationData generationData)
        {
            int count = 0;
            if (generationData.PrimaryMaterialSprite) count++;
            if (generationData.SecondaryMaterialSprite) count++;
            if (generationData.StaticSprite)  count++;
            return count;
        }
    }

    [System.Serializable]
    public class RobotArmSprites
    {
        public Sprite PrimarySprite;
        public Sprite SecondarySprite;
    }
    public enum TierGeneratedCraftingItemType
    {
        Motor = TierGeneratedItemType.Motor,
        RobotArm = TierGeneratedItemType.RobotArm
    }
    
    
    public class TierCraftingItemGenerationData
    {
        public bool UseTierName;
        public Sprite PrimaryMaterialSprite;
        public Sprite SecondaryMaterialSprite;
        public Sprite StaticSprite;
        public uint OutputAmount;
        public uint TickCount;
        public List<EditorItemSlot> BasicInputCost;
        public List<EditorItemSlot> AdvancedInputCost;

        public TierCraftingItemGenerationData(Sprite primaryMaterialSprite, Sprite secondaryMaterialSprite, Sprite staticSprite, uint outputAmount, uint tickCount, bool useTierName, List<EditorItemSlot> basicInputCost, List<EditorItemSlot> advancedInputCost)
        {
            UseTierName = useTierName;
            PrimaryMaterialSprite = primaryMaterialSprite;
            SecondaryMaterialSprite = secondaryMaterialSprite;
            StaticSprite = staticSprite;
            OutputAmount = outputAmount;
            TickCount = tickCount;
            BasicInputCost = basicInputCost;
            AdvancedInputCost = advancedInputCost;
        }
    }
}