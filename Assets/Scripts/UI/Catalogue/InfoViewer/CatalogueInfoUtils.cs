using System.Collections.Generic;
using Item.Burnables;
using Item.ItemObjects.Instances.Tile.Chisel;
using Item.ItemObjects.Instances.Tiles.Chisel;
using Item.Slot;
using Item.Transmutation.Info;
using Items.Transmutable;
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
            
            DisplayCatalogue(elements);

        }

        public static void DisplayItemUses(ItemSlot itemSlot)
        {
            var elements = RecipeViewerHelper.GetRecipesWithItem(itemSlot);

            TryAddBurnDisplay(itemSlot,elements);
            TryAddChiselDisplay(itemSlot,elements);
            
            DisplayCatalogue(elements);
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

        public static void DisplayCatalogue(List<CatalogueElementData> elements)
        {
            if (elements.Count == 0) return;
            MainCanvasController mainCanvasController = MainCanvasController.TInstance;
            if (mainCanvasController.TopHasComponent<CatalogueInfoViewer>())
            {
                mainCanvasController.PopStack();
            }
            CatalogueInfoViewer catalogueInfoViewer = mainCanvasController.DisplayUIElement<CatalogueInfoViewer>(MainSceneUIElement.CatalogueInfo);
            catalogueInfoViewer.Initialize(elements);
        }
    }
}
