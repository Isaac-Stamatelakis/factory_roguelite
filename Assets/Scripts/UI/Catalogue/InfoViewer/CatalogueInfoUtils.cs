using System.Collections.Generic;
using Item.Burnables;
using Item.ItemObjects.Instances.Tile.Chisel;
using Item.ItemObjects.Instances.Tiles.Chisel;
using Item.Slot;
using Item.Transmutation.Info;
using Items.Transmutable;
using Player;
using Recipe;
using Recipe.Viewer;
using UnityEngine;
using LinqUtility = Unity.VisualScripting.LinqUtility;

namespace UI.Catalogue.InfoViewer
{
    public static class CatalogueInfoUtils
    {
        public static void DisplayItemInformation(ItemSlot itemSlot)
        {
            var elements = RecipeViewerHelper.GetRecipesForItem(itemSlot);
            
            TryAddTransmutationDisplay(itemSlot, elements);
            TryAddChiselDisplay(itemSlot,elements);
            
            DisplayCatalogue(elements,PlayerManager.Instance.GetPlayer().GameStageCollection); // Gross singleton 

        }

        public static void DisplayItemUses(ItemSlot itemSlot)
        {
            var elements = RecipeViewerHelper.GetRecipesWithItem(itemSlot);

            TryAddBurnDisplay(itemSlot,elements);
            TryAddChiselDisplay(itemSlot,elements);
            
            DisplayCatalogue(elements,PlayerManager.Instance.GetPlayer().GameStageCollection);
        }

        private static void TryAddTransmutationDisplay(ItemSlot itemSlot, List<CatalogueElementData> elements)
        {
            if (itemSlot.itemObject is not TransmutableItemObject transmutableItemObject) return;
            
            TransmutationMaterialInfo materialInfo = new TransmutationMaterialInfo(transmutableItemObject.getMaterial());
            elements.Add(new CatalogueElementData(materialInfo,CatalogueInfoDisplayType.TransmutableMaterial));
        }

        private static void TryAddChiselDisplay(ItemSlot itemSlot, List<CatalogueElementData> elements)
        {
            if (itemSlot.itemObject is not ChiselTileItem chiselTileItem) return;
            
            ChiselCatalogueInfo chiselCatalogueInfo = new ChiselCatalogueInfo(new List<ChiselDisplayData>
                { new ChiselDisplayData(chiselTileItem.Collection) });
            elements.Add(new CatalogueElementData(chiselCatalogueInfo,CatalogueInfoDisplayType.Chisel));
        }

        private static void TryAddBurnDisplay(ItemSlot itemSlot, List<CatalogueElementData> elements)
        {
            uint burnTime = RecipeRegistry.BurnableItemRegistry.GetBurnDuration(itemSlot.itemObject);
            if (burnTime <= 0) return;
            
            BurnableItemDisplay burnableItemDisplay = new BurnableItemDisplay(itemSlot);
            BurnableInfo burnableInfo = new BurnableInfo(new List<BurnableDisplay>{burnableItemDisplay});
            elements.Add(new CatalogueElementData(burnableInfo,CatalogueInfoDisplayType.Burnable));
        }

        public static void DisplayCatalogue(List<CatalogueElementData> elements, PlayerGameStageCollection playerGameStageCollection)
        {
            FilterCatalogueUnStagedElements(elements,playerGameStageCollection);
            if (elements.Count == 0) return;
            MainCanvasController mainCanvasController = MainCanvasController.TInstance;
            if (mainCanvasController.TopHasComponent<CatalogueInfoViewer>())
            {
                mainCanvasController.PopStack();
            }
            CatalogueInfoViewer catalogueInfoViewer = mainCanvasController.DisplayUIElement<CatalogueInfoViewer>(MainSceneUIElement.CatalogueInfo);
            catalogueInfoViewer.Initialize(elements,playerGameStageCollection);
        }

        public static void FilterCatalogueUnStagedElements(List<CatalogueElementData> elements, PlayerGameStageCollection playerGameStageCollection)
        {
            for (var index = elements.Count-1; index >= 0; index--)
            {
                var catalogueElementData = elements[index];
                catalogueElementData.CatalogueElement.Filter(playerGameStageCollection);
                if (catalogueElementData.CatalogueElement.GetPageCount() == 0)
                {
                    elements.RemoveAt(index);
                }
            }
        }

        public static void FilterList<T>(List<T> elements, PlayerGameStageCollection gameStageCollection) where T : IGameStageItemDisplay
        {
            for (int j = 0; j < elements.Count; j++)
            {
                var element = elements[j];
                if (element.FilterStage(gameStageCollection)) continue;
                elements.RemoveAt(j);
            }
        }
    }
}
